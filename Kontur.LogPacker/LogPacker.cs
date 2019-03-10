using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Kontur.LogPacker
{
    internal class LogPacker
    {
        public void Compress(Stream inputStream, Stream outputStream)
        {
            using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal, true))
                inputStream.CopyTo(gzipStream);
        }

        public void Decompress(Stream inputStream, Stream outputStream)
        {
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress, true))
                gzipStream.CopyTo(outputStream);
        }
    }
}
