using System.Collections;
using System.Collections.Generic;
using Ply;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GaussianRenderer : MonoBehaviour
{
    [SerializeField] TextAsset gaussianData;
    [SerializeField] Material material;
    private NativeArray<GaussianData> gaussians;
    private GraphicsBuffer gpuData;
    private Bounds bounds;

    void Start()
    {
        if (gaussianData == null)
            return;

        gaussians = gaussianData.GetData<GaussianData>();

        bounds = new Bounds(gaussians[0].Position, Vector3.zero);
        foreach (var g in gaussians)
        {
            bounds.Encapsulate(g.Position);
        }

        int structSize = UnsafeUtility.SizeOf<GaussianData>();
        gpuData = new(GraphicsBuffer.Target.Structured, gaussians.Length, structSize);

        gpuData.SetData(gaussians);
        material.SetBuffer("_DataBuffer", gpuData);
    }

    private void Update()
    {
        if (gpuData == null)
            return;

        Graphics.DrawProcedural(material, bounds, MeshTopology.Points, 1, gaussians.Length);
    }

    private void OnDestroy()
    {
        gpuData?.Dispose();
        if (gaussians.IsCreated)
            gaussians.Dispose();
    }
}
