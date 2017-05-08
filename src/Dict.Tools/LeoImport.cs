using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Models;

namespace Import
{
    // converts chrome plugin output http://troggy.github.io/anki-leo/

    class LeoImport
    {
        public string StorePath { get; private set; }
        public string ContentRootPath { get; private set; }

        private MD5 md5;

        private OptionsWrapper<RepositoryOptions> _options;
        //private ZipArchive _arc;

        public LeoImport()
        {
            _options = new OptionsWrapper<RepositoryOptions>(new RepositoryOptions
            {
                StorePath = "..\\..\\Shared",
                DbName = "app.db"
            });
            StorePath = "..\\..\\Shared";
            ContentRootPath = "..\\..\\";

            //var arcName = StorePath + "\\media.z";
            // _arc = ZipFile.Open(arcName, ZipArchiveMode.Update);
        }

        public void ImportCsv(string fileName)
        {
            var newNames = new Dictionary<string, string>();
            using (var fs = System.IO.File.OpenText(System.IO.Path.Combine(StorePath, "newnames.csv")))
            {
                while (!fs.EndOfStream)
                {
                    var s = fs.ReadLine();
                    var fields = s.Split(',');
                    newNames.Add(fields[1], fields[0]);
                }
            }

            md5 = MD5.Create();

            Console.WriteLine("Import from {0}", fileName);
            using (var context = DictContext.CreateContext(_options))
            {
                var collection = InitCollection(context);
                var wordsDict = context.Cards.ToDictionary(c => c.Word);
                var files = new HashSet<string>(context.Files.Select(f => f.Name));

                using (var fs = System.IO.File.OpenText(System.IO.Path.Combine(StorePath, fileName)))
                {
                    while (!fs.EndOfStream)
                    {
                        var s = fs.ReadLine();
                        var fields = s.Split(';');

                        if (fields.Length != 6)
                            continue;

                        var word = fields[0];

                        //TODO: make Word as primary key?
                        //Too slow!!!
                        //var card = context.Cards.FirstOrDefault(c => string.Equals(c.Word, word, StringComparison.OrdinalIgnoreCase) && c.CollectionId != collection.Id);
                        Card card;

                        bool IsNew = !wordsDict.TryGetValue(word, out card) || card.Collection != collection;

                        if (IsNew)
                        {
                            card = new Card { Word = word, Collection = collection };
                            context.Cards.Add(card);
                            wordsDict.Add(word, card);
                        }

                        card.Translation = fields[1];
                        card.Transcription = fields[3];
                        card.Context = fields[4];

                        
                        var name = fields[2];
                        string newName;
                        if (name != "")
                        {
                            var pos = name.LastIndexOf('/');
                            name = name.Substring(pos+1);
                            if (newNames.TryGetValue(name, out newName))
                            {
                                var hash = System.IO.Path.GetFileNameWithoutExtension(newName);
                                // no hash dubbling check
                                if (!files.Contains(newName))
                                {    
                                    context.Files.Add(new FileDescription { Name = newName, FileType = FileType.Image, Hash = hash });
                                    files.Add(newName);
                                }

                                //??? take first if we get more than one word: begin, began, begun
                                string lcWord = word.Contains(",") ? word.Split(',').First().ToLower() : word.ToLower(); 
                                if (!context.WordsToFiles.Any(wf => wf.Word == lcWord && wf.FileName == newName))
                                    context.WordsToFiles.Add(new WordToFile { Word = lcWord, FileName = newName });

                                card.ImageName = newName;
                            }
                        }

                        name = fields[5];
                        if (name != "")
                        {
                            var pos = name.LastIndexOf('/');
                            name = name.Substring(pos+1);
                            if (newNames.TryGetValue(name, out newName))
                            {
                                var hash = System.IO.Path.GetFileNameWithoutExtension(newName);
                                if (!files.Contains(newName))
                                {
                                    context.Files.Add(new FileDescription { Name = newName, FileType = FileType.Sound, Hash = hash });
                                    files.Add(newName);
                                }

                                string lcWord = word.ToLower(); 
                                if (!context.WordsToFiles.Any(wf => wf.Word == lcWord && wf.FileName == newName))
                                    context.WordsToFiles.Add(new WordToFile { Word = lcWord, FileName = newName });

                                card.SoundName = newName;
                            }
                        }
                        
                        //card.Image = AddMedia(card.Image, fields[2], FileType.Image, app, srcDict, cards);
                        //card.Sound = AddMedia(card.Sound, fields[5], FileType.Sound, app, srcDict, cards);
                    }
                }

                Console.WriteLine("Save");
                context.SaveChanges();
            }
            Console.WriteLine("Done");
        }

        public void PrintDb()
        {
            // int i = 0;

            // using (var context = DictContext.CreateContext(_options))
            // {
            //     foreach (var card in context.Cards)
            //     {
            //         if (++i > 10)
            //             break;

            //         Console.WriteLine($"{card.Word} Img: {card.Image?.Source} Snd: {card.Sound?.Source}");
            //     }
            // }
        }

        private Collection InitCollection(DictContext context)
        {
            var collection = context.Collections
                .FirstOrDefault(c => c.Name == "LinguaLeo");

            if (collection == null)
            {
                collection = context.Collections.Add(new Collection {Name = "LinguaLeo", Description = "Imported LinguaLeo dictionary"}).Entity;
            }

            return collection;
        }

        // private FileDescription AddMedia(FileDescription info, string srcUrl, FileType type,
        //     AppRepository app, Dictionary<string, FileDescription> cache, CardRepository repository)
        // {
        //     if (!string.IsNullOrEmpty(srcUrl) && (info == null || info.Source != srcUrl))
        //     {
        //         if (!cache.TryGetValue(srcUrl, out info))
        //         {
        //             Uri uri;
        //             try
        //             {
        //                 uri = new Uri(srcUrl);
        //                 info = new FileDescription { Name = uri.Segments.Last(), Source = srcUrl, FileType = type };
        //             }
        //             catch (UriFormatException)
        //             {
        //                 return null;
        //             }

        //             if (!DownloadFileToStore(info, repository))
        //                 return null;

        //             app.AddFileInfo(info);
        //             cache.Add(srcUrl, info);
        //         }
        //     }


        //     return info;
        // }

        private bool DownloadFileToStore(FileDescription media, CardRepository repository)
        {
            //using (var client = new HttpClient())
            //using ()
            {
                    //TODO download and store

                    //var destFile = Path.Combine(StorePath, media.Name);
                    //HttpResponseMessage response = await client.GetAsync(uri);
                    //if (!response.IsSuccessStatusCode)
                    // {
                    //     Console.WriteLine("Download error: {0}", response.ReasonPhrase);
                    //     media.Name = "";
                    //     return;
                    // }

                    // using (var content = response.Content)
                    // using (Stream src = await content.ReadAsStreamAsync(),
                    //     dst = File.Create(destFile))
                    // {
                    //     //TODO: check src position. Is md5 threadsafe?
                    //     var hash = md5.ComputeHash(src);
                    //     media.Hash = BitConverter.ToString(hash).Replace("-", "");
                    //     //TODO: check src position
                    //     src.CopyTo(dst);
                    // }

                    return false;
            }
        }

        // public void LoadMedia()
        // {
        //     if (md5 == null)
        //         md5 = MD5.Create();

        //     using (var context = DictContext.CreateContext(_options))
        //     {
        //         var app = new Models.AppRepository(context);
        //         int i = 500;
        //         foreach (var file in app.GetFileInfos().Where(f => string.IsNullOrEmpty(f.Hash) && f.Source.StartsWith("http")))
        //         {
        //             DownloadFileAsync(file);
        //             if (--i <= 0)
        //                 break;
        //         }

        //         app.SaveChanges();
        //         Console.WriteLine("Done");
        //     }
        // }

        // private async void DownloadFileAsync(FileDescription media)
        // {
        //     var destFile = "";

        //     using (var client = new HttpClient())
        //     {
        //         Uri uri = new Uri(media.Source);
        //         media.Name = uri.Segments.Last();

        //         // TODO: check if name exists
        //         //if (media.Name)
        //         destFile = System.IO.Path.Combine(StorePath, media.Name);

        //         if (!System.IO.File.Exists(destFile))
        //         {
        //             Console.WriteLine("Load {0}", media.Name);
        //             HttpResponseMessage response = await client.GetAsync(uri);
        //             if (!response.IsSuccessStatusCode)
        //             {
        //                 Console.WriteLine("Download error: {0}", response.ReasonPhrase);
        //                 media.Name = "";
        //                 return;
        //             }

        //             using (var content = response.Content)
        //             using (System.IO.Stream src = await content.ReadAsStreamAsync(),
        //                 dst = System.IO.File.Create(destFile))
        //             {
        //                 //TODO: check src position. Is md5 threadsafe?
        //                 var hash = md5.ComputeHash(src);
        //                 media.Hash = BitConverter.ToString(hash).Replace("-", "");
        //                 //TODO: check src position
        //                 src.CopyTo(dst);
        //             }
        //         }
        //     }
        // }
    }
}
