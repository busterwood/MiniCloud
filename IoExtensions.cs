using System.IO;
using System.Text;

namespace MiniCloud
{
    static class IoExtensions
    {
        public static string ReadToEnd(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return new StreamReader(stream, Encoding.UTF8, false, 4096, true).ReadToEnd();
        }
    }
}