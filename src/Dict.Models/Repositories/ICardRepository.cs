﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Models
{
    public interface ICardRepository
    {
        // Queue
        Task<IEnumerable<Card>> GetLearnQueueAsync(int collectionId);
        // Cards
        Task<IEnumerable<Card>> GetListAsync(int collectionId, int skip, int count);
        Task<Card> AddAsync(Card item);
        Task<bool> UpdateAsync(Card item);
        Task DeleteAsync(int id);
        Task<Card> FindAsync(int id);
    }
}
