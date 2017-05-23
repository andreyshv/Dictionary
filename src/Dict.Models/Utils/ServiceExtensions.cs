using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Models
{
    public static class ModelsServiceCollectionExtensions
    {
        public static IServiceCollection AddModels(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<DictContext>((provider, builder) => DictContext.ConfigureOptionsBuilder(builder, connectionString));

            services.AddScoped<ICardRepository, CardRepository>();
            services.AddScoped<IAppRepository, AppRepository>();
            services.AddScoped<IFileRepository, FileRepository>();

            return services;
        }
    }
}