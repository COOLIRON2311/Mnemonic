using System.Collections.Generic;
using System.IO;

namespace Ply
{
    public class ScalarProperty : Property
    {
        public string Type { get; private set; }
        public List<dynamic> Values { get; private set; }
        public ScalarProperty(string name, int count, string type) : base(name)
        {
            Type = type;
            Values = new(count);
        }
        public override string ToString() => $"{Type} {Name}";
        public override void Read(BinaryReader br)
        {
            switch (Type)
            {
                case "char" or "int8":
                    Values.Add(br.ReadChar());
                    break;
                case "uchar" or "uint8":
                    Values.Add(br.ReadByte());
                    break;
                case "short" or "int16":
                    Values.Add(br.ReadInt16());
                    break;
                case "ushort" or "uint16":
                    Values.Add(br.ReadUInt16());
                    break;
                case "int" or "int32":
                    Values.Add(br.ReadInt32());
                    break;
                case "uint" or "uint32":
                    Values.Add(br.ReadUInt32());
                    break;
                case "float" or "float32":
                    Values.Add(br.ReadSingle());
                    break;
                case "double" or "float32":
                    Values.Add(br.ReadDouble());
                    break;
                default:
                    throw new InvalidDataException($"invalid type '{Type}'");
            }
        }
    }
}
