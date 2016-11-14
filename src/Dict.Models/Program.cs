using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Models
{
    // Stub for EF.Tools preview 2
    public class Program
    {
        static void Main(string[] args)
        {
        }

        // also in project.json
        //   "buildOptions": {
        //    "emitEntryPoint": true
        //  },

        // EF.Tools context factory
        public class AppContextFactory : IDbContextFactory<DictContext>
        {
            public DictContext Create(DbContextFactoryOptions options)
            {
                string dbFileName = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Shared", "app.db");

                var optionsBuilder = new DbContextOptionsBuilder<DictContext>();
                optionsBuilder.UseSqlite(string.Format("Filename={0}", dbFileName));

                return new DictContext(optionsBuilder.Options);
            }
        }
    }
}
