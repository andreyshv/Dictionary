using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Models
{
    public class CardRepository : ICardRepository
    {
        private readonly DictContext _context;
        IFileRepository _fileRepository;
        private ILogger<CardRepository> _logger;

        public CardRepository(DictContext context, IFileRepository fileRepository, ILogger<CardRepository> logger)
        {
            _context = context;
            _logger = logger;
            _fileRepository = fileRepository;
        }

        #region ICardRepository implementation
        public async Task<Card[]> GetLearnQueueAsync(int collectionId)
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
                .ToArrayAsync();

            return await GetAsync(iDs);
        }

        public async Task<Card[]> GetListAsync(int collectionId, int skip, int count)
        {
            var query = _context.Cards
                .Where(c => c.CollectionId == collectionId);

            if (skip > 0)
                query = query.Skip(skip);
            if (count > 0)
                query = query.Take(count);

            return await query.ToArrayAsync();
        }

        public async Task<Card> AddAsync(Card item)
        {
            if (!await _context.Collections.AnyAsync(c => c.Id == item.CollectionId))
            {
                return null;
            }

            await item.StoreExtFilesAsync(_fileRepository);

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

            //TODO: Card vs CardData? Restore Card's references (Image, Sound) while updating from CardData  
            if (item.ImageName == null && item.SoundName == null)
            {
                item.ImageName = stored.ImageName;
                item.SoundName = stored.SoundName;
            }

            await item.StoreExtFilesAsync(_fileRepository);

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

        private async Task<Card[]> GetAsync(int[] iDs)
        {
            var query = from card in _context.Cards
                        join id in iDs on card.Id equals id
                        select card;

            return await query.ToArrayAsync();
        }
    }
}
