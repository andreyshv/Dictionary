using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Microsoft.Extensions.FileProviders;

namespace Models.FileProviders
{
    public class CompressedFileInfo : IFileInfo
    {
        SemaphoreSlim _mutex;
        private ZipArchiveEntry _entry;

        /// <summary>
        /// Initializes a new instance of <see cref="CompressedFileInfo"/> for an assembly using <paramref name="resourcePath"/> as the base
        /// </summary>
        /// <param name="entry">Archive entry <see cref="ZipArchiveEntry"/></param>
        public CompressedFileInfo(ZipArchiveEntry entry, SemaphoreSlim mutex)
        {
            _entry = entry;
            _mutex = mutex;
        }

        /// <summary>
        /// Always true.
        /// </summary>
        public bool Exists => true;

        /// <summary>
        /// The length, in bytes, of the embedded resource
        /// </summary>
        public long Length => _entry.Length;

        /// <summary>
        /// Always null.
        /// </summary>
        public string PhysicalPath => null;

        /// <summary>
        /// The name of embedded file
        /// </summary>
        public string Name => _entry.Name;

        /// <summary>
        /// The time, in UTC, when the <see cref="EmbeddedFileProvider"/> was created
        /// </summary>
        public DateTimeOffset LastModified => _entry.LastWriteTime;

        /// <summary>
        /// Always false.
        /// </summary>
        public bool IsDirectory => false;

        /// <inheritdoc />
        public Stream CreateReadStream()
        {
            _mutex.Wait();
            try
            {            
                using (var srcStream = _entry.Open())
                {
                    var dstStream = new MemoryStream();
                    srcStream.CopyTo(dstStream);
                    dstStream.Position = 0;

                    return dstStream;
                }
            }
            finally
            {
                _mutex.Release();
            }
        }
    }
}