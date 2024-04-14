using System.Collections.Generic;
using UnityEngine;

namespace Ply
{
    public class GenericData
    {
        public List<GenericVertex> Vertices { get; protected set; }
        public class GenericVertex
        {
            public Vector3 Position { get; private set; }
            public GenericVertex(float x, float y, float z)
            {
                Position = new Vector3(x, y, z);
            }
            public override string ToString()
            {
                return $"Vertex(x={Position.x}, y={Position.y}, z={Position.z})";
            }
        }
    }
}
