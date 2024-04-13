using System.IO;
using System.Text;

namespace Ply
{
    public static class ReaderExtensions
    {
        public static string ReadLine(this BinaryReader br)
        {
            StringBuilder result = new();
            bool foundEndOfLine = false;

            char chr;
            while (!foundEndOfLine)
            {
                try
                {
                    chr = br.ReadChar();
                }
                catch (EndOfStreamException)
                {
                    if (result.Length == 0)
                        return null;
                    else
                        break;
                }

                switch (chr)
                {
                    case '\r':
                        if (br.PeekChar() == '\n')
                            br.ReadChar();
                        foundEndOfLine = true;
                        break;
                    case '\n':
                        foundEndOfLine = true;
                        break;
                    default:
                        result.Append(chr);
                        break;
                }
            }
            return result.ToString();
        }
    }
}
