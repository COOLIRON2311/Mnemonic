using System.Collections.Generic;
using Ply;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

public class GaussianRenderer : MonoBehaviour
{
    [Header("Gaussian Data Asset")]
    [SerializeField] TextAsset gaussianData;

    #region Shaders
    [Header("Shaders")]
    [SerializeField, Tooltip("Gaussian Splats rendering shader")]
    Shader splatsShader;
    [SerializeField, Tooltip("Sorting shader")]
    ComputeShader IslandGPUShader;
    [SerializeField, Tooltip("Additional rendering routines shader")]
    ComputeShader GSRoutines;
    #endregion

    #region Modifiers
    [Header("Modifiers")]
    [Range(0.1f, 2.0f), Tooltip("Splat Scale Modifier"), SerializeField]
    float scaleModifier = 1.0f;
    [Range(0, 3), Tooltip("Spherical Harmonics Degree"), SerializeField]
    int SHDegree = 3;
    #endregion

    private NativeArray<GaussianData> gaussians;
    private int splatCount;
    private int splatCountPow2;
    private GraphicsBuffer splatData;
    private GraphicsBuffer viewData;
    private GraphicsBuffer indexBuffer;
    private GraphicsBuffer sortKeys;
    private GraphicsBuffer sortDistances;
    private IslandGPUSort islandGPUSort;
    private IslandGPUSort.Args islandGPUSortArgs;
    private CommandBuffer commandBuffer;

    private Material splatsMaterial;
    // private Bounds bounds;
    private HashSet<Camera> cameraCommandBuffers;

    private bool ConfigIsValid
    {
        get
        {
            if (gaussianData == null || splatsShader == null ||
                IslandGPUShader == null || GSRoutines == null ||
                !SystemInfo.supportsComputeShaders
            )
                return false;
            return true;
        }
    }

    private void Awake()
    {
        if (!ConfigIsValid)
            return;

        cameraCommandBuffers = new();

        splatsMaterial = new Material(splatsShader) { name = "GaussianSplats" };
        islandGPUSort = new IslandGPUSort(IslandGPUShader);
        commandBuffer = new CommandBuffer { name = "GaussianRenderer" };

        gaussians = gaussianData.GetData<GaussianData>();
        splatCount = gaussians.Length;

        splatData = new GraphicsBuffer(GraphicsBuffer.Target.Structured, splatCount, UnsafeUtility.SizeOf<GaussianData>());
        splatData.SetData(gaussians);

        viewData = new GraphicsBuffer(GraphicsBuffer.Target.Structured, splatCount, UnsafeUtility.SizeOf<GSViewData>());

        indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, 6, sizeof(ushort));
        indexBuffer.SetData(new ushort[] { 0, 1, 2, 1, 3, 2 });

        splatCountPow2 = Mathf.NextPowerOfTwo(splatCount);
        sortDistances = new GraphicsBuffer(GraphicsBuffer.Target.Structured, splatCountPow2, sizeof(float)) { name = "sortDistances" };
        sortKeys = new GraphicsBuffer(GraphicsBuffer.Target.Structured, splatCountPow2, sizeof(uint)) { name = "sortKeys" };

        GSRoutines.SetInt("_CountPow2", splatCountPow2);
        GSRoutines.SetBuffer(0, "_SortKeys", sortKeys);
        GSRoutines.GetKernelThreadGroupSizes(0, out uint gs, out uint _, out uint _);
        GSRoutines.Dispatch(0, (splatCountPow2 + (int)gs - 1) / (int)gs, 1, 1);

        islandGPUSortArgs.inputKeys = sortDistances;
        islandGPUSortArgs.inputValues = sortKeys;
        islandGPUSortArgs.count = (uint)splatCountPow2;
    }

    private void OnEnable()
    {
        if (!ConfigIsValid)
            return;

        Camera.onPreCull += OnCameraPreCull;
    }

    private void OnDisable()
    {
        Camera.onPreCull -= OnCameraPreCull;
        commandBuffer?.Clear();
    }

    private void OnCameraPreCull(Camera camera)
    {
        commandBuffer?.Clear();

        Material material = splatsMaterial;

        Matrix4x4 matrix = transform.localToWorldMatrix;

        if (!cameraCommandBuffers.Contains(camera))
        {
            camera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer);
            cameraCommandBuffers.Add(camera);
        }

        SortGaussians(camera, matrix);
        ProcessGaussians(camera);

        material.SetBuffer("_OrderBuffer", sortKeys);
        material.SetBuffer("_GSViewBuffer", viewData);

        commandBuffer.DrawProcedural(indexBuffer, matrix, material, 0, MeshTopology.Triangles, 6, splatCount);
    }

    private void SortGaussians(Camera camera, Matrix4x4 localToWorldMatrix)
    {
        if (camera.cameraType == CameraType.Preview)
            return;

        Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;

        // compute distances
        commandBuffer.SetComputeIntParam(GSRoutines, "_Count", splatCount);
        commandBuffer.SetComputeIntParam(GSRoutines, "_CountPow2", splatCountPow2);
        commandBuffer.SetComputeMatrixParam(GSRoutines, "_MatrixLocalToWorld", localToWorldMatrix);
        commandBuffer.SetComputeMatrixParam(GSRoutines, "_MatrixWorldToCamera", worldToCameraMatrix);
        commandBuffer.SetComputeBufferParam(GSRoutines, 1, "_GSDataBuffer", splatData);
        commandBuffer.SetComputeBufferParam(GSRoutines, 1, "_SortKeys", sortKeys);
        commandBuffer.SetComputeBufferParam(GSRoutines, 1, "_SortDistances", sortDistances);
        GSRoutines.GetKernelThreadGroupSizes(1, out uint gs, out _, out _);
        commandBuffer.DispatchCompute(GSRoutines, 1, (splatCountPow2 + (int)gs - 1) / (int)gs, 1, 1);

        // sort splats
        islandGPUSort.Dispatch(commandBuffer, islandGPUSortArgs);
    }

    private void ProcessGaussians(Camera camera)
    {
        if (camera.cameraType == CameraType.Preview)
            return;

        Matrix4x4 matrixV = camera.worldToCameraMatrix;
        Matrix4x4 matrixP = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
        Matrix4x4 matrixLocalToWorld = transform.localToWorldMatrix;
        Matrix4x4 matrixWorldToLocal = transform.worldToLocalMatrix;

        Vector2 focal = new(
            camera.pixelWidth * matrixP.m00 / 2,
            camera.pixelWidth * matrixP.m00 / 2
        );
        Vector2 tanFov = new(1 / matrixP.m00, 1 / matrixP.m00);
        Vector3 worldCameraPos = camera.transform.position;

        GSRoutines.SetInt("_Count", splatCount);
        GSRoutines.SetInt("_SHDeg", SHDegree);
        GSRoutines.SetFloat("_ScaleModifier", scaleModifier);

        GSRoutines.SetVector("_Focal", focal);
        GSRoutines.SetVector("_TanFov", tanFov);
        GSRoutines.SetVector("_WorldCameraPos", worldCameraPos);

        GSRoutines.SetMatrix("_MatrixLocalToWorld", matrixLocalToWorld);
        GSRoutines.SetMatrix("_MatrixWorldToLocal", matrixWorldToLocal);
        GSRoutines.SetMatrix("_MatrixV", matrixV);
        GSRoutines.SetMatrix("_MatrixVP", matrixP * matrixV);
        GSRoutines.SetBuffer(2, "_GSDataBuffer", splatData);
        GSRoutines.SetBuffer(2, "_GSViewBuffer", viewData);

        GSRoutines.GetKernelThreadGroupSizes(2, out uint gs, out uint _, out uint _);
        GSRoutines.Dispatch(2, (splatCount + (int)gs - 1) / (int)gs, 1, 1);
    }

    private void OnDestroy()
    {
        if (gaussians.IsCreated)
            gaussians.Dispose();

        splatData?.Dispose();
        viewData?.Dispose();
        indexBuffer?.Dispose();
        sortKeys?.Dispose();
        sortDistances?.Dispose();
        commandBuffer?.Clear();
        cameraCommandBuffers?.Clear();
    }
}
