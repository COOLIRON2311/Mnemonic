using System.Collections.Generic;
using System.IO;

namespace Ply
{
    public class ListProperty : Property
    {
        public DType SizeType { get; private set; }
        public DType ValueType { get; private set; }
        public List<object[]> Values { get; private set; }

        public ListProperty(string name, int number, string sizeType, string valueType) : base(name)
        {
            SizeType = DTypes.FromString(sizeType);
            ValueType = DTypes.FromString(valueType);
            Values = new(number);
        }
        public override string ToString() => $"list {SizeType} {ValueType} {Name}";
        public override void Read(BinaryReader br)
        {
            var count = SizeType switch
            {
                DType.Int8 => br.ReadChar(),
                DType.UInt8 => br.ReadByte(),
                DType.Int16 => (ulong)br.ReadInt16(),
                DType.UInt16 => br.ReadUInt16(),
                DType.Int32 => (ulong)br.ReadInt32(),
                DType.UInt32 => br.ReadUInt32(),
                _ => throw new InvalidDataException($"invalid size type '{SizeType}'"),
            };

            dynamic[] arr = new dynamic[count];
            for (ulong i = 0; i < count; i++)
            {
                arr[i] = ValueType switch
                {
                    DType.Int8 => br.ReadChar(),
                    DType.UInt8 => br.ReadByte(),
                    DType.Int16 => br.ReadInt16(),
                    DType.UInt16 => br.ReadUInt16(),
                    DType.Int32 => br.ReadInt32(),
                    DType.UInt32 => br.ReadUInt32(),
                    DType.Float32 => br.ReadSingle(),
                    DType.Float64 => (dynamic)br.ReadDouble(),
                    _ => throw new InvalidDataException($"invalid value type '{ValueType}'"),
                };
            }
            Values.Add(arr);
        }
    }
}
