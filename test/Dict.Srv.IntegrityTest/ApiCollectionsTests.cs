using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.TestOrdering;
using Models;

namespace IntegrationTests
{
    // public class Program
    // {
    //     public static void Main(string[] args)
    //     {
    //         // https://github.com/aspnet/Announcements/issues/149
    //         var deps = DependencyContext.Default;
    //         Console.WriteLine($"Compilation dependencies");
    //         foreach (var compilationLibrary in deps.CompileLibraries)
    //         {
    //             Console.WriteLine($"\tPackage {compilationLibrary.Name} {compilationLibrary.Version}");
    //             // ResolveReferencePaths returns full paths to compilation assemblies
    //             foreach (var resolveReferencePath in compilationLibrary.ResolveReferencePaths())
    //             {
    //                 Console.WriteLine($"\t\tReference path: {resolveReferencePath}");
    //             }
    //         }

    //         Console.WriteLine($"Runtime dependencies");
    //         foreach (var compilationLibrary in deps.RuntimeLibraries)
    //         {
    //             Console.WriteLine($"\tPackage {compilationLibrary.Name} {compilationLibrary.Version}");
    //             foreach (var assembly in compilationLibrary.Assemblies)
    //             {
    //                 Console.WriteLine($"\t\tReference: {assembly.Name}");
    //             }
    //         }
    //     }
    // }

    public class ApiCollectionsTests : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _fixture;
        public ApiCollectionsTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            if (_fixture.Collection == null)
                _fixture.Collection = new Collection { Name = "new collection", Description = "Descr" };
        }

        [Fact]
        public async Task CollectionAdd()
        {
            // Add Item

            // PostAsJsonAsync - ref Microsoft.AspNet.WebApi.Client pkg
            // http://www.asp.net/web-api/overview/advanced/calling-a-web-api-from-a-net-client
            var response = await _fixture.Client.PostAsJsonAsync("/api/collections", _fixture.Collection);
            response.EnsureSuccessStatusCode();

            var item = await response.Content.ReadAsJsonAsync<Collection>();

            Assert.NotEqual(0, item.Id);
            _fixture.Collection.Id = item.Id;

            Assert.Equal(_fixture.Collection.Name, item.Name);
            Assert.Equal(_fixture.Collection.Description, item.Description);
        }

        [Fact]
        [DependOn(nameof(CollectionAdd))]
        public async Task CollectionUpdate()
        {
            // Update item

            _fixture.Collection.Name = "Updated";
            _fixture.Collection.Description = "Updated Item";
            var response = await _fixture.Client.PutAsJsonAsync("/api/collections/" + _fixture.Collection.Id, _fixture.Collection);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        [DependOn(nameof(CollectionUpdate))]
        public async Task CollectionGetItem()
        {
            // Get Item

            var response = await _fixture.Client.GetAsync("/api/collections/" + _fixture.Collection.Id);
            response.EnsureSuccessStatusCode();

            var item2 = await response.Content.ReadAsJsonAsync<Collection>();

            Assert.Equal(_fixture.Collection.Id, item2.Id);
            Assert.Equal(_fixture.Collection.Name, item2.Name);
            Assert.Equal(_fixture.Collection.Description, item2.Description);
        }

        [Fact]
        [DependOn(nameof(CollectionGetItem))]
        public async Task CollectionDelete()
        {
            // Remove Item

            var response = await _fixture.Client.DeleteAsync("/api/collections/" + _fixture.Collection.Id);
            response.EnsureSuccessStatusCode();
        }
    }
}
