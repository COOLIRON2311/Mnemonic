using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ply
{
    public class GaussianData : GenericData
    {
        // public List<Vertex> Vertices { get; private set; }
        public GaussianData(BinaryPlyReader reader)
        {
            Element vertices = reader.Data[0];
            Vertices = new(vertices.Count);

            var propMap = vertices.Properties
                .Select((p, idx) => (idx, p))
                .ToDictionary(x => x.p.Name, x => x.idx);

            var x = vertices.Properties[propMap["x"]] as ScalarProperty;
            var y = vertices.Properties[propMap["y"]] as ScalarProperty;
            var z = vertices.Properties[propMap["z"]] as ScalarProperty;
            var nx = vertices.Properties[propMap["nx"]] as ScalarProperty;
            var ny = vertices.Properties[propMap["ny"]] as ScalarProperty;
            var nz = vertices.Properties[propMap["nz"]] as ScalarProperty;
            var f_dc_0 = vertices.Properties[propMap["f_dc_0"]] as ScalarProperty;
            var f_dc_1 = vertices.Properties[propMap["f_dc_1"]] as ScalarProperty;
            var f_dc_2 = vertices.Properties[propMap["f_dc_2"]] as ScalarProperty;
            var opacity = vertices.Properties[propMap["opacity"]] as ScalarProperty;
            var scale_0 = vertices.Properties[propMap["scale_0"]] as ScalarProperty;
            var scale_1 = vertices.Properties[propMap["scale_1"]] as ScalarProperty;
            var scale_2 = vertices.Properties[propMap["scale_2"]] as ScalarProperty;
            var rot_0 = vertices.Properties[propMap["rot_0"]] as ScalarProperty;
            var rot_1 = vertices.Properties[propMap["rot_1"]] as ScalarProperty;
            var rot_2 = vertices.Properties[propMap["rot_2"]] as ScalarProperty;
            var rot_3 = vertices.Properties[propMap["rot_3"]] as ScalarProperty;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertices.Add(new Vertex(
                    (float)x.Values[i], (float)y.Values[i], (float)z.Values[i],
                    (float)nx.Values[i], (float)ny.Values[i], (float)nz.Values[i],
                    (float)scale_0.Values[i], (float)scale_1.Values[i], (float)scale_2.Values[i],
                    (float)rot_0.Values[i], (float)rot_1.Values[i], (float)rot_2.Values[i], (float)rot_3.Values[i],
                    (float)f_dc_0.Values[i], (float)f_dc_1.Values[i], (float)f_dc_2.Values[i],
                    (float)opacity.Values[i]
                ));
            }
        }

        #region Vertex
        public class Vertex : GenericVertex
        {
            // public Vector3 Position { get; private set; }
            public Vector3 Normal { get; private set; }
            public Vector4 Color { get; private set; }
            public Vector3 Scale { get; private set; }
            public Vector4 Rotation { get; private set; }
            public Vertex(
                float x, float y, float z,
                float nx, float ny, float nz,
                float scale_0, float scale_1, float scale_2,
                float rot_0, float rot_1, float rot_2, float rot_3,
                float f_dc_0, float f_dc_1, float f_dc_2, float opacity) : base(x, y, z)
            {
                // Position = new Vector3(x, y, z);
                Normal = new Vector3(nx, ny, nz);

                const double SH_C0 = 0.28209479177387814;
                Color = new Vector4(
                    (float)(0.5 + SH_C0 * f_dc_0),
                    (float)(0.5 + SH_C0 * f_dc_1),
                    (float)(0.5f + SH_C0 * f_dc_2),
                    1 / (1 + Mathf.Exp(-opacity))
                );

                Scale = new Vector3(scale_0, scale_1, scale_2);
                Rotation = new Vector4(rot_0, rot_1, rot_2, rot_3);
            }
        }
        #endregion
    }
}
