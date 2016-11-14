using System.Threading.Tasks;
using Xunit;
using Models;
using System.Net;

namespace IntegrationTests
{
    public class ApiCardsTests : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _fixture;

        public ApiCardsTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Add()
        {
            var item = new CardData
            {
                word = "card 1",
                transcription = "transcription 1",
                translation = "translation 1",
                collectionId = 1
            };
            
            var response = await _fixture.Client.PostAsJsonAsync("/api/cards", item);
            response.EnsureSuccessStatusCode();

            var card = await response.Content.ReadAsJsonAsync<CardData>();

            Assert.NotEqual(0, card.id);
            Assert.Equal(item.collectionId, card.collectionId);
            Assert.Equal(item.word, card.word);
        }

        [Fact]
        public async Task GetList()
        {
            var response = await _fixture.Client.GetAsync("/api/cards/collection/1");
            response.EnsureSuccessStatusCode();
            var cards = await response.Content.ReadAsJsonAsync<CardData[]>();

            Assert.Equal(10, cards.Length); // default list size
        }

        [Fact]
        public async Task GetListWithParams()
        {
            var response = await _fixture.Client.GetAsync("/api/cards/collection/1?first=5&count=5");
            response.EnsureSuccessStatusCode();
            var cards = await response.Content.ReadAsJsonAsync<CardData[]>();

            Assert.Equal(5, cards.Length);
        }

        [Fact]
        public async Task GetById()
        {
            var response = await _fixture.Client.GetAsync("/api/cards/1");
            response.EnsureSuccessStatusCode();
            var card = await response.Content.ReadAsJsonAsync<CardData>();

            Assert.Equal(1, card.id);
        }

        [Fact]
        public async Task Update()
        {
            var item = new CardData
            {
                id = 1,
                word = "updated",
                transcription = "transcription 1",
                translation = "translation 1",
                collectionId = 1
            };
            
            var response = await _fixture.Client.PutAsJsonAsync("/api/cards/1", item);
            response.EnsureSuccessStatusCode();

            response = await _fixture.Client.GetAsync("/api/cards/1");
            response.EnsureSuccessStatusCode();
            var card = await response.Content.ReadAsJsonAsync<CardData>();

            Assert.Equal("updated", card.word);
        }

        [Fact]
        public async Task Delete()
        {
            var response = await _fixture.Client.GetAsync("/api/cards/2");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response = await _fixture.Client.DeleteAsync("/api/cards/2");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            response = await _fixture.Client.GetAsync("/api/cards/2");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}