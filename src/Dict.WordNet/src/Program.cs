using System;
using System.IO;
using System.Linq;
using Dict.WordNet.Model;
using Microsoft.EntityFrameworkCore;

namespace Dict.WordNet.Tools
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dbFileName = Path.GetFullPath(dbFile);

            // convert wn database to sqlite
            //convToSQLite(dbFileName);

            // Convert db to json format
            var jsonFile = Path.Combine(Path.GetDirectoryName(dbFile), "wordnet.bson");
            JsonConverter.Convert(dictPack, jsonFile);

            // using (var context = GetContext(dbFileName))
            // {
            //     var s = Search.GetSearch("indices", true, PartOfSpeech.of("noun"), context);
            // }

            // Console.WriteLine("Press any key...");
            // Console.ReadKey();
        }

        public const string dbFile = @"..\..\shared\wordnet.db";
        private const string dictPack = @"..\..\shared\wordnet\dict.zip";

        public static void convToSQLite(string dbFileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dbFileName));

            using (var context = GetContext(dbFileName))
            {
                //Console.WriteLine("Migrate Db");
                //context.Database.Migrate();
                context.Database.EnsureCreated();
                if (!context.Lemmas.Any())
                {
                    Console.WriteLine("Convert");
                    Tools.Converter.Convert(dictPack, context);
                }
                else
                    Console.WriteLine("Db already converted");
            }
        }

        private static WordNetContext GetContext(string dbFileName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WordNetContext>();
            optionsBuilder.UseSqlite(string.Format("Filename={0}", dbFileName));

            return new WordNetContext(optionsBuilder.Options);
        }
    }
}
