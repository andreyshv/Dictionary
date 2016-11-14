using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Xunit;

namespace Dict.Models.UnitTest
{
    public class CardRepositoryTests
    {
        private static DbContextOptions<DictContext> CreateNewContextOptions()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var builder = new DbContextOptionsBuilder<DictContext>();
            builder.UseInMemoryDatabase()
                   .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }

        private int SeedDb(DbContextOptions<DictContext> options, int count, bool expand)
        {
            int cId;
            using (var context = new DictContext(options))
            {
                var c = context.Collections.Add(new Collection { Name = "collection" });
                cId = c.Entity.Id;

                var m0 = context.Medias.Add(new Media { Name = "m0" }).Entity;
                var m1 = context.Medias.Add(new Media { Name = "m1" }).Entity;

                for (int i = 0; i < count; i++)
                {
                    context.Cards.Add(new Card { Word = string.Format("w{0}", i * 2), CollectionId = cId, Sound = m0, Image = (i % 2 == 0 ? m1 : null) });
                    if (expand)
                    {
                        context.Cards.Add(new Card { Word = string.Format("w{0}", i * 2 + 1), CollectionId = cId + 1 });
                    }
                }
                context.SaveChanges();
            }

            return cId;
        }

        private object GetPropValue(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj);
        }

        [Fact]
        public void LearnQueue()
        {
            var options = CreateNewContextOptions();
            int cId = SeedDb(options, 12, true);

            using (var context = new DictContext(options))
            {
                var repo = new CardRepository(context);

                var r = repo.GetLearnQueueAsync(cId).Result;
                Assert.Equal(10, r.Count());
            }
        }

        [Fact]
        public void List_With_No_Args()
        {
            var options = CreateNewContextOptions();
            int cId = SeedDb(options, 3, true);

            using (var context = new DictContext(options))
            {
                var repo = new CardRepository(context);
                var r = repo.GetListAsync(cId, 0, 0).Result;

                Assert.Equal(3, r.Count());
            }
        }

        [Fact]
        public void List_With_Args()
        {
            var options = CreateNewContextOptions();
            int cId = SeedDb(options, 6, true);

            using (var context = new DictContext(options))
            {
                // Logging.ConsoleLoggerProvider.RegisterLogger(context);

                var repo = new CardRepository(context);

                var r = repo.GetListAsync(cId, 0, 5).Result;
                Assert.Equal(5, r.Count());

                r = repo.GetListAsync(cId, 5, 5).Result;
                Assert.Equal(1, r.Count());
            }
        }

        [Fact]
        public async void Add()
        {
            //Given
            var options = CreateNewContextOptions();
            SeedDb(options, 0, false);

            using (var context = new DictContext(options))
            {
                var media = context.Medias.First();
                var collectionId = context.Collections.First().Id;
                var item = new Card { Word = "word", CollectionId = collectionId, Image = media };

                var repo = new CardRepository(context);
                var item2 = await repo.AddAsync(item);

                // Not null & has Id
                Assert.NotNull(item2);
                var id2 = item2.id;
                Assert.NotEqual(0, id2);

                Assert.Equal(1, context.Cards.Count());
                var item3 = context.Cards.FirstOrDefault();

                // Not null & has same Id
                Assert.NotNull(item3);
                Assert.Equal(id2, item3.Id);
                Assert.Equal(item3.Word, item.Word);
                Assert.Equal(item3.CollectionId, collectionId);
                Assert.Equal(item3.ImageId, media.Id);
            }
        }

        [Fact]
        public async void Update()
        {
            //Given
            var options = CreateNewContextOptions();
            SeedDb(options, 1, false);

            using (var context = new DictContext(options))
            {
                var item = context.Cards.FirstOrDefault();
                var id = item.Id;

                var repo = new CardRepository(context);
                item.Word = "updated word";
                var res = await repo.UpdateAsync(item);

                var item2 = context.Cards.FirstOrDefault(c => c.Id == id);

                Assert.Equal(true, res);
                Assert.Equal("updated word", item2.Word);
            }
        }

        [Fact]
        public async void Update_wrong_item()
        {
            //Given
            var options = CreateNewContextOptions();
            SeedDb(options, 1, false);

            using (var context = new DictContext(options))
            {
                var id = context.Cards.FirstOrDefault().Id;

                var repo = new CardRepository(context);
                var item2 = new Card {Id = id+1, Word = "updated word" };
                var res = await repo.UpdateAsync(item2);

                Assert.Equal(false, res);
            }
        }

        [Fact]
        public async void Delete()
        {
            //Given
            var options = CreateNewContextOptions();
            SeedDb(options, 1, false);

            using (var context = new DictContext(options))
            {
                var item = context.Cards.FirstOrDefault();
                
                var repo = new CardRepository(context);
                await repo.DeleteAsync(item.Id);

                Assert.Equal(0, context.Cards.Count());
            }
        }

        [Fact]
        public async void Find()
        {
            //Given
            var options = CreateNewContextOptions();
            SeedDb(options, 1, false);

            using (var context = new DictContext(options))
            {
                var item = context.Cards.FirstOrDefault();

                var repo = new CardRepository(context);
                var item2 = await repo.FindAsync(item.Id);

                var id =  item2.id;
                Assert.Equal(item.Id, id);
            }
        }
    }
}