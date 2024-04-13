using System.IO;

namespace Ply
{
    public abstract class Property
    {
        public string Name { get; private set; }
        public Property(string name)
        {
            Name = name;
        }
        public abstract void Read(BinaryReader br);
    }
}
