using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ply;
using System.IO;
using UnityEditor;


public enum PlyType { GaussianData, PointCloudData }

public class GaussianSplatsLoader : MonoBehaviour
{
    public GameObject primitive;
    public string plyFilePath;
    public PlyType plyType;
    private void Awake()
    {
        LoadDemoScene();
    }

    private void LoadDemoScene()
    {
        var reader = new BinaryPlyReader(plyFilePath);
        reader.ReadAll();

        GenericData data = plyType switch
        {
            PlyType.GaussianData => new GaussianData(reader),
            PlyType.PointCloudData => new PointCloudData(reader),
            _ => throw new InvalidDataException()
        };
        reader.Clear();

        for (int i = 0; i < data.Vertices.Count; i++)
        {
            GameObject obj = Instantiate(primitive, gameObject.transform);
            var vertex = data.Vertices[i];
            obj.transform.position = vertex.Position;
            obj.name = vertex.ToString();
        }
    }
}
