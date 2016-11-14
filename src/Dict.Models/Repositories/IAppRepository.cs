using System.Threading.Tasks;

namespace Models
{
    public interface IAppRepository
    {
        ICollectionRepository GetCollectionRepository();
        //System.Collections.IEnumerable GetCardSet();
        Task<bool> SetRepetitionAsync(int cardId, int quality);
        Task ResetProgressAsync();
    }
}
