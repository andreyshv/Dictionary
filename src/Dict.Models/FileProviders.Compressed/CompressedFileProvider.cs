using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

//TODO: Add UnitTest

namespace Models.FileProviders
{
    /// <summary>
    /// Looks up files in single compressed file.
    /// This file provider is case sensitive.
    /// </summary>
    public class CompressedFileProvider : IFileProvider, IDisposable
    {
        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars()
            .Where(c => c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar).ToArray();

        private ILogger _logger;
        private SemaphoreSlim _mutex = new SemaphoreSlim(1, 1);
        private ZipArchive _arc;

        public CompressedFileProvider(string arcName, ILogger logger)
        {
            _logger = logger;

            try
            {
                _arc = ZipFile.OpenRead(arcName);
            }
            catch (FileNotFoundException)
            {
                _arc = null;
                //_logger.LogError($"Archive not found: {arcName}");
            }
        }

        /// <summary>
        /// Locates a file at the given path.
        /// </summary>
        /// <param name="subpath">The path that identifies the file. </param>
        /// <returns>
        /// The file information. Caller must check Exists property. A <see cref="NotFoundFileInfo" /> if the file could
        /// not be found.
        /// </returns>
        public IFileInfo GetFileInfo(string subpath)
        {
            if (_arc == null || string.IsNullOrEmpty(subpath))
            {
                //_logger.LogWarning("Archive not opened or empty path");
                return new NotFoundFileInfo(subpath);
            }

            if (HasInvalidPathChars(subpath))
            {
                // _logger.LogWarning($"Invalid path chars: {subpath}");
                return new NotFoundFileInfo(subpath);
            }

            if (subpath[0] == '/')
            {
                subpath = subpath.Remove(0, 1);
            }

            ZipArchiveEntry entry = null;
            _mutex.Wait();
            try 
            {
                entry = _arc.GetEntry(subpath);
            } 
            finally
            {
                _mutex.Release();
            }

            if (entry == null)
            {
                // _logger.LogWarning($"Entry not found: {subpath}");
                return new NotFoundFileInfo(subpath);
            }

            // _logger.LogInformation($"Open entry: {subpath}");
            return new CompressedFileInfo(entry, _mutex);
        }

        /// <summary>
        /// Enumerate a directory at the given path, if any.
        /// </summary>
        /// <param name="subpath">The path that identifies the directory</param>
        /// <returns>
        /// Contents of the directory. Caller must check Exists property. A <see cref="NotFoundDirectoryContents" /> if no
        /// resources were found that match <paramref name="subpath" />
        /// </returns>
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (_arc == null)
            {
                // _logger.LogWarning("Archive not opened");
                return new NotFoundDirectoryContents();
            }

            var entries = _arc.Entries
                .Where(e => e.FullName.StartsWith(subpath ?? string.Empty))
                .Select(e => new CompressedFileInfo(e, _mutex));

            // _logger.LogDebug($"Directory content readed: {subpath}");
            return new EnumerableDirectoryContents(entries);
        }

        /// <summary>
        /// Compressed files do not change.
        /// </summary>
        /// <param name="pattern">This parameter is ignored</param>
        /// <returns>A <see cref="NullChangeToken" /></returns>
        public IChangeToken Watch(string pattern)
        {
            return NullChangeToken.Singleton;
        }

        public void Dispose()
        {
            _arc.Dispose();
        }

        private static bool HasInvalidPathChars(string path)
        {
            return path.IndexOfAny(_invalidFileNameChars) != -1;
        }
    }
}