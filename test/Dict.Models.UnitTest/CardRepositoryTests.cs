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
        private DbContextOptions<DictContext> CreateNewContextOptions()
        {
            // Create a fresh service provider, and therefore a fresh 
            // InMemory database instance.
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance telling the context to use an
            // InMemory database and the new service provider.
            var options = new DbContextOptionsBuilder<DictContext>()
                .UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            return options;
        }

        private int SeedDb(DbContextOptions<DictContext> options, int count, bool create2Collections)
        {
            int collectionId;
            using (var context = new DictContext(options))
            {
                var c = context.Collections.Add(new Collection { Name = "collection" });
                collectionId = c.Entity.Id;

                for (int i = 0; i < count; i++)
                {
                    var snd = context.Files.Add(new FileDescription { Name = $"sound{i}.mp3" }).Entity;
                    var img = (i % 2 == 0) ? context.Files.Add(new FileDescription { Name = $"image{i}.png" }).Entity : null;

                    context.Cards.Add(new Card { 
                        Word = $"word{i * 2}", 
                        CollectionId = collectionId, 
                        SoundName = snd.Name, 
                        ImageName =  img?.Name 
                    });

                    if (create2Collections)
                    {
                        context.Cards.Add(new Card { 
                            Word = $"word{count + i}",
                            CollectionId = collectionId + 1 
                        });
                    }
                }

                context.SaveChanges();
            }

            return collectionId;
        }

        private DictContext GetContext(int count, bool expand, out int collectionId)
        {
            var options = CreateNewContextOptions();
            collectionId = SeedDb(options, count, expand);
            
            return new DictContext(options);
        }

        /*private object GetPropValue(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj);
        }*/

        private ICardRepository GetRepository(DictContext context)
        {
            var logger = Logging.ConsoleLoggerProvider.CreateLogger<CardRepository>();
            var fileRepository = new InMemoryFileRepository();
            var repository = new CardRepository(context, fileRepository, logger);

            return repository;
        }

        [Fact]
        public void GetLearnQueue()
        {
            using (var context = GetContext(12, true, out int collectionId))
            {
                var repository = GetRepository(context);

                var r = repository.GetLearnQueueAsync(collectionId).Result;
                Assert.Equal(10, r.Count());
            }
        }

        [Theory]
        [InlineData(0, 0, 6)]
        [InlineData(0, 5, 5)]
        [InlineData(5, 5, 1)]
        public void GetList(int offs, int count, int got)
        {
            using (var context = GetContext(6, true, out int collectionId))
            {
                // Logging.ConsoleLoggerProvider.RegisterLogger(context);
                var repository = GetRepository(context);

                var r = repository.GetListAsync(collectionId, offs, count).Result;
                Assert.Equal(got, r.Count());
            }
        }

        [Fact]
        public async void Add()
        {
            using (var context = GetContext(0, false, out int collectionId))
            {
                var fileName = "file.ext";
                var item1 = new Card { Word = "word", CollectionId = collectionId, ImageName = fileName };

                var repository = GetRepository(context);
                var item2 = await repository.AddAsync(item1);

                // Not null & has Id
                Assert.NotNull(item2);
                var id2 = item2.Id;
                Assert.NotEqual(0, id2);

                Assert.Equal(1, context.Cards.Count());
                var item3 = context.Cards.FirstOrDefault();

                // Not null & has same Id
                Assert.NotNull(item3);
                Assert.Equal(id2, item3.Id);
                Assert.Equal(item1.Word, item3.Word);
                Assert.Equal(collectionId, item3.CollectionId);
                Assert.Equal(fileName, item3.ImageName);
            }
        }

        [Fact]
        public async void Update()
        {
            //Given
            using (var context = GetContext(1, false, out int collectionId))
            {
                var item1 = context.Cards.FirstOrDefault();
                var id = item1.Id;

                Assert.NotNull(item1.SoundName);
                
                var soundName = item1.SoundName;
                var imageULR = "/" + FileRepository.MEDIA_DIR + "/new-image.png";
                item1.Word = "updated word";
                item1.ImageURL = imageULR;

                var repository = GetRepository(context);
                var res = await repository.UpdateAsync(item1);

                var item2 = context.Cards.FirstOrDefault(c => c.Id == id);

                Assert.True(res);
                Assert.Equal("updated word", item2.Word);
                Assert.Equal(imageULR, item2.ImageURL);
                Assert.Equal("new-image.png", item2.ImageName);
                Assert.Equal(soundName, item2.SoundName);
            }
        }

        //TODO:
        /*[Fact]
        public async void Update_with_image_url()
        {
            using (var context = GetContext(1, false, out int collectionId))
            {
                var fileName = "/" + FileRepository.MEDIA_DIR + "/file.ext";
                var item1 = new Card { Word = "word", CollectionId = collectionId, ImageURL = fileName };

                var repository = GetRepository(context);
                var item2 = await repository.AddAsync(item1);

                Assert.Equal("file.ext", item2.ImageName);
            }
        }*/

        [Fact]
        public async void Update_wrong_item()
        {
            //Given
            using (var context = GetContext(1, false, out int collectionId))
            {
                var id = context.Cards.FirstOrDefault().Id;

                var repository = GetRepository(context);
                var item2 = new Card {Id = id+1, Word = "updated word" };
                var res = await repository.UpdateAsync(item2);

                Assert.False(res);
            }
        }

        [Fact]
        public async void Delete()
        {
            //Given
            using (var context = GetContext(1, false, out int collectionId))
            {
                //Logging.ConsoleLoggerProvider.RegisterLogger(context);
                var item = context.Cards.First();
                
                var repository = GetRepository(context);
                await repository.DeleteAsync(item.Id);

                Assert.Equal(0, context.Cards.Count());
            }
        }

        [Fact]
        public async void Find()
        {
            //Given
            using (var context = GetContext(3, false, out int collectionId))
            {
                var item1 = context.Cards.Last();

                var repository = GetRepository(context);
                var item2 = await repository.FindAsync(item1.Id);

                Assert.Equal(item1.Id, item2.Id);
            }
        }
    }
}