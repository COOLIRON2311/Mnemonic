using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ply;

public class GaussianSplatsLoader : MonoBehaviour
{
    private PlyReader reader;
    private void Awake()
    {
        reader = new(".vscode/test.ply");
        reader.ReadAll();
    }
}
