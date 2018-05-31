using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace Backend.Utils
{
    public class BrotliCompressionProvider : ICompressionProvider
    {
        public Stream CreateStream(Stream outputStream)
        {
            return new BrotliStream(outputStream, CompressionLevel.Optimal, true);
        }

        public string EncodingName => "br";

        public bool SupportsFlush => true;
    }
}