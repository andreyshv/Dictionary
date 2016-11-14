using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Models;

namespace WebDictionary
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Setup options
            services.AddOptions();
            services.Configure<RepositoryOptions>(Configuration.GetSection("Repository"));
            
            services.AddModels();

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            // Setup dev environment
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();

                loggerFactory.AddDebug(LogLevel.Debug);

                //FIX: Microsoft.AspNetCore.NodeServices:Error: ts-loader: Using typescript@2.0.3 and c:\my\VS-Projects\Dictionary\src\Dict.Site\tsconfig.json

                // https://github.com/aspnet/JavaScriptServices/tree/dev/src/Microsoft.AspNetCore.SpaServices#enabling-webpack-dev-middleware
                // var projPath = Path.GetFullPath(@"..\Dict.Site");
                // app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                //     ProjectPath = projPath,
                //     HotModuleReplacement = true
                // });
            }

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DictContext>();       
                if (context.Database.EnsureCreated())
                {
                    context.SeedData();
                }
            } 

            var options = new RepositoryOptions();
            Configuration.GetSection("Repository").Bind(options);

            // File provider

            app.UseStaticFiles(new StaticFileOptions 
            {   //TODO: use single place to store paths
                FileProvider = new Models.FileProviders.CompressedFileProvider(arcName, loggerFactory.CreateLogger("CompressedFileProvider")),
                RequestPath = new PathString(Models.CardRepository.STATIC_DIR) 
            });

            app.UseMvc();
        }
    }
}
