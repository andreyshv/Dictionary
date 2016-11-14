using System.Threading.Tasks;
using Xunit;
using Models;

namespace IntegrationTests
{
    public class ApiQueuesTests : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _fixture;
        public ApiQueuesTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task QueuesGet()
        {
            var response = await _fixture.Client.GetAsync("/api/queues/1");
            response.EnsureSuccessStatusCode();

            var res = await response.Content.ReadAsJsonAsync<CardData[]>();
            Assert.Equal(10, res.Length); // default queue size
            // Assert.Equal("a", res[0].word);
            // Assert.Equal("", res[0].transcription);
            // Assert.Equal("", res[0].imageURL);

            // Assert.Equal("b", res[1].word);
        }
    }
}
