using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Models;

namespace IntegrationTests
{
    public class DatabaseFixture : IDisposable
    {
        private readonly TestServer _server;
        
        public HttpClient Client {get; private set;}

        public Collection Collection;
        //public CardData Card;

        private const string TEST_DB_NAME = "test.db";
        private const string SEED_DB_NAME = "seed.db";
        private string _dbPath;

        public DatabaseFixture()
        {
            // init db
            _dbPath = Path.Combine(Directory.GetCurrentDirectory(), "test", "Data");

            File.Copy(Path.Combine(_dbPath, SEED_DB_NAME), Path.Combine(_dbPath, TEST_DB_NAME), true);

            _server = new TestServer(new WebHostBuilder()
                .UseContentRoot(_dbPath) // set appsettings.json location
                .UseStartup<WebDictionary.Startup>()
            );

            Client = _server.CreateClient();

            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();

            File.Delete(Path.Combine(_dbPath, TEST_DB_NAME));
        }
    }
}