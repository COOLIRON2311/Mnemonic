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

            if (plyType == PlyType.PointCloudData)
            {
                var vx = vertex as PointCloudData.Vertex;
                var material = obj.GetComponent<Renderer>().material;
                material.color = new Color(
                    Mathf.Clamp(vx.Color[0] / 255.0f, 0.0f, 1.0f),
                    Mathf.Clamp(vx.Color[1] / 255.0f, 0.0f, 1.0f),
                    Mathf.Clamp(vx.Color[2] / 255.0f, 0.0f, 1.0f)
                );
            }
        }
    }
}
