using System.IO;

namespace Ply
{
    public enum DType
    {
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Float32,
        Float64
    }

    public static class DTypes
    {
        public static DType FromString(string type)
        {
            return type switch
            {
                "char" or "int8" => DType.Int8,
                "uchar" or "uint8" => DType.UInt8,
                "short" or "int16" => DType.Int16,
                "ushort" or "uint16" => DType.UInt16,
                "int" or "int32" => DType.Int32,
                "uint" or "uint32" => DType.UInt32,
                "float" or "float32" => DType.Float32,
                "double" or "float64" => DType.Float64,
                _ => throw new InvalidDataException($"invalid type '{type}'")
            };
        }

        public static int SizeOf(DType dType)
        {
            return dType switch
            {
                DType.Int8 or DType.UInt8 => 1,
                DType.Int16 or DType.UInt16 => 2,
                DType.Int32 or DType.UInt32 or DType.Float32 => 4,
                DType.Float64 => 8,
                _ => throw new InvalidDataException($"invalid type '{dType}'")
            };
        }
    }
}
