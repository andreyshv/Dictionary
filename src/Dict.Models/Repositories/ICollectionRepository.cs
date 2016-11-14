using System.Collections.Generic;
using System.Threading.Tasks;

namespace Models
{
    public interface ICollectionRepository
    {
        Task<IEnumerable<Collection>> GetCollectionsAsync();
        Task<Collection> GetCollectionAsync(int id);
        Collection Add(string name, string desription);
        Task<Collection> AddAsync(Collection item);
        Task DeleteAsync(int id);
        Task<bool> UpdateAsync(Collection item);
    }
}
