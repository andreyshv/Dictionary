using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Security.Cryptography;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class FileRepository: IFileRepository
    {
        private readonly DictContext _context;
        private readonly string _storePath;
        private ILogger<FileRepository> _logger;
        //TODO: check MD5.Create() performance
        private static ThreadLocal<MD5> _localMD5 = new ThreadLocal<MD5>(() => MD5.Create());

        public const string MEDIA_DIR = "media";

        public FileRepository(DictContext context, IHostingEnvironment env, ILogger<FileRepository> logger)
        {
            _context = context;
            _storePath = Path.Combine(env.ContentRootPath, MEDIA_DIR);
            _logger = logger;
        }

        #region IFileRepository implementation
        // Add archive file, every file named by associated word
        public int AddArchive(Stream stream, string origin)
        {   
            _logger.LogDebug($"AddArchive(); Origin: {origin}");
            int count = 0;

            try
            {
                using (var archive = new ZipArchive(stream))
                {
                    foreach (var entry in archive.Entries)
                    {
                        _logger.LogDebug($"Read Entry: {entry.Name} Length: {entry.Length}");
                        using (var entryStream = entry.Open())
                        {
                            // ...

                            count++;
                        }
                    }
                }
            } 
            catch (Exception e)
            {
                _logger.LogWarning("Can't read archive: {0}", e.Message);
            }

            return count;
        }

        // Add single file associated with word
        public bool AddFile(Stream file, string word)
        {
            throw new NotImplementedException();
        }

        // Add single file associated with word
        public async Task<string> AddFileAsync(string url, string word)
        {
            Uri uri;
            try
            {
                uri = new Uri(url);
                using (var client = new HttpClient())
                {
                    _logger.LogDebug("Download File {0}", url);
                    HttpResponseMessage response = await client.GetAsync(uri);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Can't Download File: {0}", response.ReasonPhrase);
                        return null;
                    }

                    using (var content = response.Content)
                    {
                        //? content.Headers.ContentMD5
                        var mediaType = content.Headers.ContentType.MediaType;
                        if (!mediaType.StartsWith("image/"))
                        {
                            _logger.LogInformation("Wrong Conent Type: {0}", mediaType);
                            return null;
                        }

                        var ext = mediaType.Substring(6);

                        using (Stream src = await content.ReadAsStreamAsync())
                        {
                            var hash = BitConverter.ToString(_localMD5.Value.ComputeHash(src))
                               .Replace("-", "");
                            src.Position = 0;

                            var fileName = $"{hash}.{ext}";
                            var destFile = Path.Combine(_storePath, fileName);
                            using (var dst = File.Create(destFile))
                            {
                                await src.CopyToAsync(dst);
                            }

                            if (!_context.Files.Any(fi => fi.Name == fileName))
                            {
                                _context.Files.Add(new FileDescription { Name = fileName, Hash = hash, FileType = FileType.Image });
                            }

                            AddWordToFileItem(word, fileName);

                            return fileName;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Download File Error: {0}", e.Message);
            }

            return null;
        }
        
        public async Task<WordToFile[]> GetByWordAsync(string word)
        {
            //TODO fuzzy search
            var query = from wf in _context.WordsToFiles
                where wf.Word == word
                select wf;

            return await query.ToArrayAsync();
        }
        #endregion

        private void AddWordToFileItem(string word, string fileName)
        {
            //TODO use wordnet to get main word form
            if (!_context.WordsToFiles.Any(wf => wf.Word == word && wf.FileName == fileName))
            {
                _context.WordsToFiles.Add(new WordToFile { Word = word, FileName = fileName });
            }
        }
    }
}