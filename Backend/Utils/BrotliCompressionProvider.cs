using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace Backend.Utils
{
    public class BrotliCompressionProvider : ICompressionProvider
    {
        private readonly CompressionLevel _compressionLevel;
        
        public BrotliCompressionProvider(CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            _compressionLevel = compressionLevel;
        }

        public Stream CreateStream(Stream outputStream)
        {
            return new BrotliStream(outputStream, _compressionLevel, true);
        }

        public string EncodingName => "br";

        public bool SupportsFlush => true;
    }
}