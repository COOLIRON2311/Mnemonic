using System.Collections.Generic;
using System.IO;

namespace Ply
{
    public class ListProperty : Property
    {
        public string SizeType { get; private set; }
        public string ValueType { get; private set; }
        public List<dynamic[]> Values { get; private set; }

        public ListProperty(string name, string sizeType, string valueType) : base(name)
        {
            SizeType = sizeType;
            ValueType = valueType;
            Values = new();
        }
        public override string ToString() => $"list {SizeType} {ValueType} {Name}";
        public override void Read(BinaryReader br)
        {
            var count = SizeType switch
            {
                "char" or "int8" => br.ReadChar(),
                "uchar" or "uint8" => br.ReadByte(),
                "short" or "int16" => (ulong)br.ReadInt16(),
                "ushort" or "uint16" => br.ReadUInt16(),
                "int" or "int32" => (ulong)br.ReadInt32(),
                "uint" or "uint32" => br.ReadUInt32(),
                _ => throw new InvalidDataException($"invalid size type '{SizeType}'"),
            };

            dynamic[] arr = new dynamic[count];
            for (ulong i = 0; i < count; i++)
            {
                arr[i] = ValueType switch
                {
                    "char" or "int8" => br.ReadChar(),
                    "uchar" or "uint8" => br.ReadByte(),
                    "short" or "int16" => br.ReadInt16(),
                    "ushort" or "uint16" => br.ReadUInt16(),
                    "int" or "int32" => br.ReadInt32(),
                    "uint" or "uint32" => br.ReadUInt32(),
                    "float" or "float32" => br.ReadSingle(),
                    "double" or "float32" => (dynamic)br.ReadDouble(),
                    _ => throw new InvalidDataException($"invalid value type '{ValueType}'"),
                };
            }
            Values.Add(arr);
        }
    }
}
