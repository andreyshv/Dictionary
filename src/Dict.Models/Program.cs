using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Design;

namespace Models
{
    // Stub for EF.Tools
    public class Program
    {
        static void Main(string[] args)
        {
        }

        // EF.Tools context factory
        public class AppContextFactory : IDesignTimeDbContextFactory<DictContext>
        {
            public DictContext CreateDbContext(string[] args)
            {
                string dbFileName = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Shared", "app.db");

                var optionsBuilder = new DbContextOptionsBuilder<DictContext>();
                optionsBuilder.UseSqlite(string.Format("Filename={0}", dbFileName));

                return new DictContext(optionsBuilder.Options);
            }
        }
    }
}
