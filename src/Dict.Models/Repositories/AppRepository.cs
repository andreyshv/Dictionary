using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Models
{
    public class AppRepository : IAppRepository
    {
        private DictContext _context;
    
        public AppRepository(DictContext context)
        {
            _context = context;
        }

        #region Tool Methods
        public ICollectionRepository GetCollectionRepository()
        {
            return new CollectionRepository(_context);
        }

        public IQueryable<FileDescription> GetFileInfos()
        {
            return _context.Files;
        }

        public void AddFileInfo(FileDescription info)
        {
            _context.Files.Add(info);    
        }
        #endregion

        public void AddCard(Card card)
        {
            _context.Cards.Add(card);    
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        private Settings _settings;

        public Settings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = _context.Settings.FirstOrDefault();
                    if (_settings == null)
                    {
                        _settings = new Settings();
                        _settings.SetDefaults();
                        _context.Settings.Add(_settings);
                    }
                }

                return _settings;
            }
        }

        #region IAppRepository implementation
        public Task<bool> SetRepetitionAsync(int cardId, int quality)
        {
            return Task.Run(() =>
            {
                var repetition = _context.Repetitions.Where(r => r.CardId == cardId).FirstOrDefault();
                if (repetition == null)
                    return false;

                var now = DateTime.Now;
                var rnd = new Random(); //TODO: move to class level or global MT RandomGenerator 

                if (repetition.Interval < 1 || quality < 3) // Iteration == 0
                {
                    repetition.Interval = 1;
                    repetition.EasynessFactior = MaxEasynessFactior;
                }
                else if (repetition.Interval < 6) // Iteration == 1
                {
                    repetition.Interval = 6;
                }
                else
                {
                    repetition.Interval *= repetition.EasynessFactior;
                    // check quality range [0..5]
                    int q = 5 - Math.Max(Math.Min(quality, 5), 0);
                    repetition.EasynessFactior *= (float)(0.1 - q * (0.08 + q * 0.02));
                }

                repetition.NextRepetition = now.AddDays(repetition.Interval).AddSeconds(rnd.Next(300));
                repetition.Iteration++;

                _context.SaveChanges();

                return true;
            });
        }

        public async Task ResetProgressAsync()
        {
            await _context.Repetitions.ForEachAsync(r =>
                {
                    r.EasynessFactior = MaxEasynessFactior;
                    r.Interval = 0;
                    r.Iteration = 0;
                    r.NextRepetition = DateTime.MinValue;
                });

            await _context.SaveChangesAsync();
        }
        #endregion

        private const float MaxEasynessFactior = 2.5f;

        private void SyncWithCards()
        {
            //TODO: remove this, testing only 
            // fill Repetitions if empty
            if (!_context.Repetitions.Any())
            {
                var repetitions = _context.Cards
                    .Select(c => new Repetition
                    {
                        CardId = c.Id,
                        EasynessFactior = MaxEasynessFactior
                    });

                _context.Repetitions.AddRange(repetitions);
                _context.SaveChanges();

                return;
            }
        }
    }
}
