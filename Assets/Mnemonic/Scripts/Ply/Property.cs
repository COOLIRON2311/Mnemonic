using System;

namespace Ply
{
    public abstract class Property
    {
        public string Name { get; private set; }
        public Property(string name)
        {
            Name = name;
        }
        public abstract int Size { get; }
    }

    public class ScalarProperty : Property
    {
        public DType Type { get; private set; }
        public ScalarProperty(string name, string type) : base(name)
        {
            Type = DTypes.FromString(type);
        }
        public override string ToString() => $"{Type} {Name}";
        public override int Size => DTypes.SizeOf(Type);
    }

    // public class ListProperty : Property
    // {
    //     public DType SizeType { get; private set; }
    //     public DType ValueType { get; private set; }

    //     public ListProperty(string name, string sizeType, string valueType) : base(name)
    //     {
    //         SizeType = DTypes.FromString(sizeType);
    //         ValueType = DTypes.FromString(valueType);
    //     }
    //     public override string ToString() => $"list {SizeType} {ValueType} {Name}";
    //     public override int SizeOf() => throw new NotImplementedException();
    // }
}
