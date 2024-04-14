using System.Collections.Generic;
using UnityEngine;

namespace Ply
{
    public class PointCloudData : GenericData
    {
        public PointCloudData(BinaryPlyReader reader)
        {
            Element vertices = reader.Data[0];
            Vertices = new(vertices.Count);

            var x = vertices.Properties[0] as ScalarProperty;
            var y = vertices.Properties[1] as ScalarProperty;
            var z = vertices.Properties[2] as ScalarProperty;
            var nx = vertices.Properties[3] as ScalarProperty;
            var ny = vertices.Properties[4] as ScalarProperty;
            var nz = vertices.Properties[5] as ScalarProperty;
            var red = vertices.Properties[6] as ScalarProperty;
            var green = vertices.Properties[7] as ScalarProperty;
            var blue = vertices.Properties[8] as ScalarProperty;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertices.Add(new Vertex(
                    (float)x.Values[i], (float)y.Values[i], (float)z.Values[i],
                    (float)nx.Values[i], (float)ny.Values[i], (float)nz.Values[i],
                    (byte)red.Values[i], (byte)green.Values[i], (byte)blue.Values[i]
                ));
            }
        }

        #region Vertex
        public class Vertex : GenericVertex
        {
            // public Vector3 Position {get; private set; }
            public Vector3 Normal { get; private set; }
            public Vector3Int Color { get; private set; }

            public Vertex(
                float x, float y, float z,
                float nx, float ny, float nz,
                byte red, byte green, byte blue) : base(x, y, z)
            {
                // Position = new Vector3(x, y, z);
                Normal = new Vector3(nx, ny, nz);
                Color = new Vector3Int(red, green, blue);
            }
        }
        #endregion
    }
}
