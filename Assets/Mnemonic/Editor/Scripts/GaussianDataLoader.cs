using UnityEngine;
using Ply;
using System.IO;
using UnityEditor;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading.Tasks;

public static class GaussianDataLoader
{
    [MenuItem("Tools/Create GaussianDataAsset")]
    public static void CreateGaussianDataAsset()
    {
        string plyFilePath = EditorUtility.OpenFilePanel("Open PLY File", "", "ply");
        if (!PlyFilePathIsValid(plyFilePath))
            return;

        string assetPath = EditorUtility.SaveFilePanel(
            "Open Output Path", string.Empty,
            Path.GetFileNameWithoutExtension(plyFilePath),
            "bytes"
        );

        assetPath = ValidateConvertOutputPath(assetPath);
        if (assetPath == null)
            return;

        BinaryPlyReader reader = new(plyFilePath);
        reader.Parse();

        if (reader.Elements.Count < 1 || reader.Elements[0].Properties.Count != 62)
        {
            Debug.LogError($"Could not parse '{plyFilePath}'");
            return;
        }

        var gaussians = reader.Elements[0].GetData<GaussianData>();

        TransposeSHRestMat(gaussians);

        ModifyRotationScaleOpacity(gaussians);

        CreateBinary(assetPath, gaussians);

        Debug.Log($"Asset '{assetPath}' created successfully");
    }

    /// <summary>
    /// Transpose f_rest matrix embedded in GaussianData struct
    /// </summary>
    /// <remarks>
    /// <see cref="https://github.com/graphdeco-inria/gaussian-splatting/blob/main/scene/gaussian_model.py#L245"/>
    /// </remarks>
    /// <param name="data">Array of gaussians to process</param>
    private static void TransposeSHRestMat(NativeArray<GaussianData> gaussians)
    {
        int structSize = UnsafeUtility.SizeOf<GaussianData>();
        int offset = 3 * 3; // SH matrix offset in struct
        int shCount = 15; // number of SH coefficient triplets to transpose
        var tmp = new float[shCount * 3]; // transposed matrix buffer
        NativeArray<float> a = gaussians.Reinterpret<float>(structSize);
        int stride = structSize / sizeof(float); // exact number of floats in struct

        int idx = offset;
        for (int i = 0; i < gaussians.Length; i++)
        {
            for (int j = 0; j < shCount; j++)
            {
                tmp[j * 3 + 0] = a[idx + j]; // 1st row
                tmp[j * 3 + 1] = a[idx + shCount + j]; // 2nd row
                tmp[j * 3 + 2] = a[idx + shCount * 2 + j]; // 3rd row
            }

            for (int j = 0; j < shCount * 3; j++)
            {
                a[idx + j] = tmp[j];
            }

            idx += stride;
        }
    }

    private static void CreateBinary(string path, NativeArray<GaussianData> gaussians)
    {
        int structSize = UnsafeUtility.SizeOf<GaussianData>();
        NativeArray<byte> bytes = gaussians.Reinterpret<byte>(structSize);

        using var stream = File.Open(path, FileMode.OpenOrCreate);
        using var writer = new BinaryWriter(stream);

        writer.Write(bytes);
    }

    private static void ModifyRotationScaleOpacity(NativeArray<GaussianData> gaussians)
    {
        Parallel.For(0, gaussians.Length, i =>
        {
            GaussianData g = gaussians[i];

            // Rotation
            Quaternion r = g.Rotation.normalized;
            g.Rotation = new(r.y, r.z, r.w, r.x);

            // Scale
            Vector3 s = g.Scale;
            g.Scale = new(
                Mathf.Exp(s.x),
                Mathf.Exp(s.y),
                Mathf.Exp(s.z)
            );

            // Opacity
            g.Opacity = Sigmoid(g.Opacity);

            gaussians[i] = g;
        });
    }

    private static float Sigmoid(float v)
    {
        return 1.0f / (1.0f + Mathf.Exp(-v));
    }

    private static bool PlyFilePathIsValid(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;
        return true;
    }

    private static string ValidateConvertOutputPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Debug.LogError($"Invalid path '{path}'");
            return null;
        }

        path = FileUtil.GetProjectRelativePath(path);

        if (!path.StartsWith("Assets"))
        {
            Debug.LogError("Output path must be a subdirectory of the project's 'Assets' directory");
            return null;
        }

        return path;
    }
}
