using System.Collections.Generic;
using System.IO;
using Unity.Collections;

namespace Ply
{
    public class Element
    {
        public string Name { get; private set; }
        public int Count { get; private set; }
        public List<Property> Properties { get; private set; }
        public NativeArray<byte> Data => data;
        private int stride;
        private NativeArray<byte> data;
        public Element(string name, int count)
        {
            Name = name;
            Count = count;
            Properties = new();
            stride = 0;
        }

        public override string ToString() => $"{Name} {Count}";

        public void UpdateStride(Property p) => stride += p.Size;

        public void Read(BinaryReader br)
        {
            data = new(br.ReadBytes(stride * Count), Allocator.Temp);
        }

        /// <summary>
        /// Reinterpret raw data as custom struct
        /// </summary>
        /// <typeparam name="T">struct type</typeparam>
        /// <returns>Raw data as NativeArray of target type</returns>
        public NativeArray<T> GetData<T>() where T: struct
        {
            return data.Reinterpret<T>(sizeof(byte));
        }

        ~Element()
        {
            if (data.IsCreated)
                data.Dispose();
        }
    }

}
