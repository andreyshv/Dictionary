using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Models
{
    public class CardRepository : ICardRepository
    {
        private readonly DictContext _context;
        private readonly string _storePath;
        private ILogger<CardRepository> _logger;

        public CardRepository(DictContext context, IHostingEnvironment env, ILogger<CardRepository> logger)
        {
            _context = context;
            _storePath = Path.Combine(env.ContentRootPath, MEDIA_DIR);
            _logger = logger;
        }

        public CardRepository(DictContext context, string contentRootPath, ILogger<CardRepository> logger)
        {
            _context = context;
            _storePath = Path.Combine(contentRootPath, MEDIA_DIR);
            _logger = logger;
        }

        public const string MEDIA_DIR = "media";

        #region ICardRepository implementation
        public async Task<IEnumerable<Card>> GetLearnQueueAsync(int collectionId)
        {
            if (!await _context.Collections.AnyAsync(c => c.Id == collectionId))
            {
                return null;
            }

            var query = from c in _context.Cards
                        join r in _context.Repetitions on c.Id equals r.CardId into grp
                        from sr in grp.DefaultIfEmpty()
                        where c.CollectionId == collectionId && sr == null
                        select c.Id;

            var iDs = await query
                .Take(10)
                .ToListAsync();

            return await GetAsync(iDs);
        }

        public async Task<IEnumerable<Card>> GetListAsync(int collectionId, int skip, int count)
        {
            var query = _context.Cards
                .Where(c => c.CollectionId == collectionId);

            if (skip > 0)
                query = query.Skip(skip);
            if (count > 0)
                query = query.Take(count);

            return await query.ToListAsync();
        }

        public async Task<Card> AddAsync(Card item)
        {
            if (!await _context.Collections.AnyAsync(c => c.Id == item.CollectionId))
            {
                return null;
            }

            if (item.ExtImageURL != null)
            {
                await DownloadFileAsync(item.ExtImageURL, newId => item.ImageName = newId);
            }

            _context.Cards.Add(item);
            await _context.SaveChangesAsync();

            return await FindAsync(item.Id);
        }

        public async Task<bool> UpdateAsync(Card item)
        {
            var stored = await _context.Cards.FirstOrDefaultAsync(c => c.Id == item.Id);
            if (stored == null)
                return false;

            _context.Entry(stored).State = EntityState.Detached;

            //TODO: Card vs CardData? Update Card's references (Image, Sound) while updating from CardData  
            if (item.ImageName == null && item.SoundName == null)
            {
                item.ImageName = stored.ImageName;
                item.SoundName = stored.SoundName;
            }

            if (item.ExtImageURL != null)
            {
                await DownloadFileAsync(item.ExtImageURL, newId => item.ImageName = newId);
            }

            _context.Cards.Update(item);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.Cards.FirstOrDefaultAsync(c => c.Id == id);
            if (item != null)
            {
                var res = _context.Cards.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Card> FindAsync(int id)
        {
            var query = _context.Cards
                .Where(c => c.Id == id);

            var list = await query.ToListAsync();

            return list.FirstOrDefault();
        }
        #endregion

        // used by Dict.Tools
        public IQueryable<Card> GetCards()
        {
            return _context.Cards;
        }

        private async Task<IEnumerable<Card>> GetAsync(List<int> iDs)
        {
            var query = from card in _context.Cards
                        join id in iDs on card.Id equals id
                        select card;

            return await query.ToListAsync();
        }

        //TODO: check MD5.Create() perfomance
        private static ThreadLocal<MD5> _localMD5 = new ThreadLocal<MD5>(() => MD5.Create());
        private async Task DownloadFileAsync(string srcUrl, Action<string> updateName)
        {
            Uri uri;
            try
            {
                uri = new Uri(srcUrl);
                using (var client = new HttpClient())
                {
                    _logger.LogDebug("Download File {0}", srcUrl);
                    HttpResponseMessage response = await client.GetAsync(uri);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Can't Download File: {0}", response.ReasonPhrase);
                        return;
                    }

                    using (var content = response.Content)
                    {
                        //? content.Headers.ContentMD5
                        var medaType = content.Headers.ContentType.MediaType;
                        if (!medaType.StartsWith("image/"))
                        {
                            _logger.LogInformation("Wrong Conent Type: {0}", medaType);
                            return;
                        }

                        var ext = medaType.Substring(6);

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

                            updateName(fileName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Download File Error: {0}", e.Message);
            }
        }
    }
}
