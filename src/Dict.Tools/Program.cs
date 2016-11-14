namespace Dict.Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            var ll = new Import.LeoImport();
            ll.ImportCsv("lingualeo-dict-export.csv");
            //ll.LoadMedia();
            //ll.PrintDb();

            // Text tools 
            //EpubParser.ConvertEpubToTxt(@"c:\my\Книги\pocketbook\English", @"c:\my\Книги\pocketbook\English-txt");
            //TextParser.ParseFiles(@"c:\my\Книги\pocketbook\English-txt", @"..\..\shared\words.txt");
            
            // TestWordNet.TestSqlite(System.IO.Path.GetFullPath(@"..\..\shared\wordnet.db"), 
            //     @"..\..\shared\words.txt", @"..\..\shared\words-exc.txt");

            // TestWordNet.TestJson(@"..\..\shared\wordnet.bson", @"..\..\shared\words.txt");

            //Console.ReadKey();
        }
    }
}
