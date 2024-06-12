using System;
using System.Collections.Generic;
using System.IO;

namespace Ply
{
    public class ScalarProperty : Property
    {
        public DType Type { get; private set; }
        public List<object> Values { get; private set; }
        public ScalarProperty(string name, int count, string type) : base(name)
        {
            Type = DTypes.FromString(type);
            Values = new(count);
        }
        public override string ToString() => $"{Type} {Name}";
        public override void Read(BinaryReader br)
        {
            switch (Type)
            {
                case DType.Int8:
                    Values.Add(br.ReadChar());
                    break;
                case DType.UInt8:
                    Values.Add(br.ReadByte());
                    break;
                case DType.Int16:
                    Values.Add(br.ReadInt16());
                    break;
                case DType.UInt16:
                    Values.Add(br.ReadUInt16());
                    break;
                case DType.Int32:
                    Values.Add(br.ReadInt32());
                    break;
                case DType.UInt32:
                    Values.Add(br.ReadUInt32());
                    break;
                case DType.Float32:
                    Values.Add(br.ReadSingle());
                    break;
                case DType.Float64:
                    Values.Add(br.ReadDouble());
                    break;
                default:
                    throw new InvalidDataException($"invalid type '{Type}'");
            }
        }
    }
}
