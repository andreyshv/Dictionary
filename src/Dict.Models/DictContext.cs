using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

        public static DbContextOptionsBuilder ConfigureOptionsBuilder(DbContextOptionsBuilder builder, string connectionString)
        {
            return builder.UseSqlite(connectionString);
        }

        //TODO: remove this?
        public static DictContext CreateContext(string connectionString, bool createNew)
        {
            //var storePath = System.IO.Path.GetFullPath(options.Value.StorePath);
            //var dbName = options.Value.DbName;
            //string dbFileName = System.IO.Path.Combine(storePath, string.IsNullOrEmpty(dbName) ? "app.db" : dbName);
            //var connectionString = string.Format("Filename={0}", dbFileName);

            var builder = new DbContextOptionsBuilder<DictContext>();
            builder.UseSqlite(connectionString);

            var context = new DictContext(builder.Options);
            if (createNew)
            {
                context.Database.EnsureDeleted();
            }

            if (context.Database.EnsureCreated())
            {
                context.SeedData();
            }

            return context;
        }

        public void SeedData()
        {
            Collections.Add(new Collection {Name = "My collection", Description = "Default collection"});
            SaveChanges();
        }
    }
}
