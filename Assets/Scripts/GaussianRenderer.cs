using System.Collections;
using System.Collections.Generic;
using Ply;
using Unity.Collections;
using UnityEngine;

public class GaussianRenderer : MonoBehaviour
{
    [SerializeField] TextAsset gaussianData;
    NativeArray<GaussianData> gaussians;

    void Start()
    {
        if (gaussianData == null)
            return;

        gaussians = gaussianData.GetData<GaussianData>();
        print(gaussians.Length);
    }
}
