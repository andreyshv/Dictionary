using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Models;

namespace IntegrationTests
{
    public class DatabaseFixture : IDisposable
    {
        private readonly TestServer _server;
        
        public HttpClient Client {get; private set;}

        public Collection Collection;

        private string _connString;

        public DatabaseFixture()
        {
            // init db
            //_dbPath = Path.Combine(Directory.GetCurrentDirectory(), "test", "Data");

            // use same connection string as WebDictionary.Startup class
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            _connString = configuration.GetConnectionString("DefaultConnection");
            SeedDb(20);

            _server = new TestServer(new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory()) // set appsettings.json location
                .UseStartup<WebDictionary.Startup>()
            );

            Client = _server.CreateClient();

            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void SeedDb(int count)
        {
            using (var context = DictContext.CreateContext(_connString, true))
            {
                var c = context.Collections.First();

                for (int i = 0; i < count; i++)
                {
                    var snd = context.Files.Add(new FileDescription { Name = $"sound{i}.mp3" }).Entity;
                    var img = (i % 2 == 0) ? context.Files.Add(new FileDescription { Name = $"image{i}.png" }).Entity : null;

                    context.Cards.Add(new Card { 
                        Word = $"word{i * 2}", 
                        CollectionId = c.Id, 
                        SoundName = snd.Name, 
                        ImageName =  img?.Name 
                    });
                }

                context.SaveChanges();
            }
        }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();

            // delete database
            using (var context = DictContext.CreateContext(_connString, false))
            {
                context.Database.EnsureDeleted();
            }

            // remove media directory
            System.IO.Directory.Delete(Models.FileRepository.MEDIA_DIR);
        }
    }
}