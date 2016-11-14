using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

// https://docs.asp.net/en/latest/tutorials/first-web-api.html

namespace Models
{
    internal class CollectionRepository : ICollectionRepository
    {
        private readonly DictContext _context;

        public CollectionRepository(DictContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Collection>> GetCollectionsAsync()
        {
            return await _context.Collections.ToListAsync();
        }

        public async Task<Collection> GetCollectionAsync(int id)
        {
            return await _context.Collections.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Collection> AddAsync(Collection item)
        {
            _context.Collections.Add(item);
            await _context.SaveChangesAsync();

            return item;
        }

        public async Task<bool> UpdateAsync(Collection item)
        {
            if (!await _context.Collections.AnyAsync(c => c.Id == item.Id))
                return false;

            _context.Update(item);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public Collection Add(string name, string desription)
        {
            var item = new Collection { Name = name, Description = desription };
            _context.Collections.Add(item);
            _context.SaveChanges();

            return item;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.Collections.FirstOrDefaultAsync(c => c.Id == id);
            if (item != null)
            {
                _context.Collections.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}