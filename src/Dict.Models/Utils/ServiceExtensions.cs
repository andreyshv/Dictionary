using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Models
{
    public static class ModelsServiceCollectionExtensions
    {
        public static IServiceCollection AddModels(this IServiceCollection serviceCollection)
        {
            //TODO: use connection string 
            serviceCollection.AddDbContext<DictContext>(
                (provider, builder) => {
                    var options = provider.GetService<IOptions<RepositoryOptions>>();
                    var storePath = Path.GetFullPath(options.Value.StorePath);
                    var dbName = options.Value.DbName;

                    string dbFileName = Path.Combine(storePath, string.IsNullOrEmpty(dbName) ? "app.db" : dbName);
                    builder.UseSqlite(string.Format("Filename={0}", dbFileName));
                } 
            );

            serviceCollection.AddScoped<ICardRepository, CardRepository>();
            serviceCollection.AddScoped<IAppRepository, AppRepository>();

            return serviceCollection;
        }

        // public static IServiceCollection AddModels(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction)
        // {
             
        //     serviceCollection.AddDbContext<DictContext>(optionsAction);

        //     serviceCollection.AddScoped<ICardRepository, CardRepository>();
        //     serviceCollection.AddScoped<IAppRepository, AppRepository>();

        //     return serviceCollection;
        // }
    }
}