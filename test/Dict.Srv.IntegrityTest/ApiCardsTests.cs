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
            var item = new Card
            {
                Word = "card 1",
                Transcription = "transcription 1",
                Translation = "translation 1",
                CollectionId = 1
            };
            
            var response = await _fixture.Client.PostAsJsonAsync("/api/cards", item);
            response.EnsureSuccessStatusCode();

            var card = await response.Content.ReadAsJsonAsync<Card>();

            Assert.NotEqual(0, card.Id);
            Assert.Equal(item.CollectionId, card.CollectionId);
            Assert.Equal(item.Word, card.Word);
        }

        [Theory]
        [InlineData("", 10)] // 10 - default list size
        [InlineData("?first=5&count=5", 5)]
        public async Task GetList(string parameters, int resultLength)
        {
            var response = await _fixture.Client.GetAsync("/api/cards/collection/1" + parameters);
            response.EnsureSuccessStatusCode();
            var cards = await response.Content.ReadAsJsonAsync<Card[]>();

            Assert.Equal(resultLength, cards.Length); 
        }

        [Fact]
        public async Task GetById()
        {
            var response = await _fixture.Client.GetAsync("/api/cards/1");
            response.EnsureSuccessStatusCode();
            var card = await response.Content.ReadAsJsonAsync<Card>();

            Assert.Equal(1, card.Id);
        }

        [Fact]
        public async Task Update()
        {
            var item = new Card
            {
                Id = 1,
                Word = "updated",
                Transcription = "transcription 1",
                Translation = "translation 1",
                CollectionId = 1
            };
            
            var response = await _fixture.Client.PutAsJsonAsync("/api/cards/1", item);
            response.EnsureSuccessStatusCode();

            response = await _fixture.Client.GetAsync("/api/cards/1");
            response.EnsureSuccessStatusCode();
            var card = await response.Content.ReadAsJsonAsync<Card>();

            Assert.Equal("updated", card.Word);
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