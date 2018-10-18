using System;

namespace Dict.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var ll = new Import.LeoImport();
            ll.ImportCsv("lingualeo-dict-export.csv");
            */

            //ll.LoadMedia();
            //ll.PrintDb();

            // Text tools 
            //EpubParser.ConvertEpubToTxt(@"c:\my\Книги\pocketbook\English", @"c:\my\Книги\pocketbook\English-txt");
            //TextParser.ParseFiles(@"c:\my\Книги\pocketbook\English-txt", @"..\..\shared\words.txt");
            
            // TestWordNet.TestSqlite(System.IO.Path.GetFullPath(@"..\..\shared\wordnet.db"), 
            //     @"..\..\shared\words.txt", @"..\..\shared\words-exc.txt");

            // TestWordNet.TestJson(@"..\..\shared\wordnet.bson", @"..\..\shared\words.txt");

            var pf = new PostFile();
            var url = "http://localhost:5000/api/files";
            var file = "/home/andy/dev/Dictionary/shared/exclude/sound-pack.zip";
            
            Console.WriteLine("Start upload");
            var startTime = DateTime.Now;

            pf.UploadAsync(url, file, null, "zip-archive").Wait();
            
            var span = DateTime.Now - startTime;
            Console.WriteLine($"End upload in {span}");
            //Console.ReadKey();
        }
    }
}
