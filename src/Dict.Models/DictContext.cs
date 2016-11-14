using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// EF Core: https://docs.efproject.net/en/latest/intro.html

namespace Models
{
    public class DictContext : DbContext
    {
        public DbSet<Collection> Collections { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<WordToFile> WordsToFiles { get; set; }
        public DbSet<FileDescription> Files { get; set; }
        public DbSet<Repetition> Repetitions { get; set; }

        public DbSet<Settings> Settings { get; set; }

        public DictContext(DbContextOptions<DictContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WordToFile>()
                .HasIndex(b => b.Word);
        }

        //TODO: remove this?
        public static DictContext CreateContext(IOptions<RepositoryOptions> options)
        {
            var storePath = System.IO.Path.GetFullPath(options.Value.StorePath);
            var dbName = options.Value.DbName;

            string dbFileName = System.IO.Path.Combine(storePath, string.IsNullOrEmpty(dbName) ? "app.db" : dbName);
            
            var builder = new DbContextOptionsBuilder<DictContext>();
            builder.UseSqlite(string.Format("Filename={0}", dbFileName));

            var context = new DictContext(builder.Options);
            if (context.Database.EnsureCreated())
            {
                context.SeedData();
            }

            return context;
        }

        public void SeedData()
        {
            Collections.Add(new Collection {Name = "My collection", Description = "Default coolection"});
        }
    }
}
