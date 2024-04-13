using System.Collections.Generic;

namespace Ply
{
    public class Element
    {
        public string Name { get; private set; }
        public int Count { get; private set; }
        public List<Property> Properties { get; private set; }
        public Element(string name, int count)
        {
            Name = name;
            Count = count;
            Properties = new();
        }

        public override string ToString() => $"{Name} {Count}";
    }

}
