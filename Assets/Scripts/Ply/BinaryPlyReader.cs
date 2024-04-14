using System.Collections.Generic;
using System.IO;
using System.Linq;
using Util;

namespace Ply
{
    public class BinaryPlyReader
    {
        private readonly string path;
        private FileStream fs;
        private BinaryReader br;
        public List<Element> Data { get; private set; }

        public BinaryPlyReader(string path)
        {
            this.path = path;
            Data = new();
        }

        public void ReadAll()
        {
            fs = File.Open(path, FileMode.Open);
            br = new BinaryReader(fs);

            long offset = ReadHeader();
            ReadData(offset);

            br.Close();
            fs.Close();
        }

        public void Clear() => Data.Clear();

        private long ReadHeader()
        {
            bool first_line = true;
            string line = string.Empty;

            // Header
            while (!line.Equals("end_header"))
            {
                line = br.ReadLine();

                // Check for ply header word
                if (first_line)
                {
                    if (!line.StartsWith("ply"))
                        throw new InvalidDataException("missing 'ply' in header");
                    else
                        first_line = false;
                }

                // Check format
                if (line.StartsWith("format"))
                {
                    var tokens = line.Split(' ').Skip(1).ToArray();
                    string format = tokens[0];
                    string version = tokens[1];

                    if (!format.Equals("binary_little_endian"))
                        throw new InvalidDataException($"format '{format}' is not supported");

                    if (!version.Equals("1.0"))
                        throw new InvalidDataException($"version {version} is not supported");
                }

                // Element declaration
                if (line.StartsWith("element"))
                {
                    var tokens = line.Split(' ').Skip(1).ToArray();
                    string name = tokens[0];
                    int count = int.Parse(tokens[1]);
                    Data.Add(new Element(name, count));
                }

                // Property declaration
                if (line.StartsWith("property"))
                {
                    var tokens = line.Split(' ').Skip(1).ToArray();

                    // List property
                    if (tokens[0].Equals("list"))
                    {
                        string sizeType = tokens[1];
                        string valueType = tokens[2];
                        string name = tokens[3];

                        Element element = Data[^1];
                        element.Properties.Add(new ListProperty(name, element.Count, sizeType, valueType));
                    }

                    // Scalar property
                    else
                    {
                        string type = tokens[0];
                        string name = tokens[1];

                        Element element = Data[^1];
                        element.Properties.Add(new ScalarProperty(name, element.Count, type));
                    }
                }
            }
            return fs.Position;
        }

        private void ReadData(long offset)
        {
            fs.Seek(offset, SeekOrigin.Begin);

            // Data
            foreach (var element in Data)
            {
                for (int i = 0; i < element.Count; i++)
                {
                    foreach (var property in element.Properties)
                    {
                        if (property is ScalarProperty scalar)
                            scalar.Read(br);

                        else if (property is ListProperty list)
                            list.Read(br);
                    }
                }
            }
        }
    }
}
