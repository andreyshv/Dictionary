using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dict.WordNet.Model;

namespace Dict.Tools
{
    public class TestWordNet
    {
        public static void TestJson(string fileName, string wordsFile)
        {
            var timer = new DebugTimer();
            var mem = new MemUsage();

            var storage = WordNet.Json.DictStorage.Open(fileName);
            
            mem.MemChange(); // ~56mb (+ 10mb wordList)
            timer.Stop("Bson open"); // bson ~2s (json ~13s)

            var lookupWord = "abl";
            Console.WriteLine($"Lookup for {lookupWord}:");
            foreach (var w in storage.Lookup(lookupWord).Take(10))
            { 
                Console.WriteLine(w);
            }
            Console.WriteLine();
            
        }

        public static void TestSqlite(string wordNetDb, string wordsFile, string notFoundFile)
        {
            Console.WriteLine("Start");
            var wordSet = new HashSet<string>(File.ReadAllLines(wordsFile));
            // var notFoundList = new List<string>();
            int found = 0;
            int morphs = 0;
            int notFound = 0;
            int cnt = 0;
            var start = DateTime.Now;

            using (var context = WordNetContext.GetContext(wordNetDb))
            {
                foreach (var word in wordSet)
                {
                    if (word.Trim() == "")
                        continue;

                    var search = Search.GetSearch(word, context);
                    if (search.SynSets.Any())
                    {
                        // Console.WriteLine($"+ {word}");
                        found++;
                    }
                    else if (search.MorphStrings?.Any() ?? false)
                    {
                        // Console.WriteLine($"* {word} -> {search.MorphStrings.First()}");
                        morphs++;
                    }
                    else
                    {
                        // Console.WriteLine($"- {word}");
                        // notFoundList.Add(word);
                        notFound++;
                    }

                    if (cnt == 0)
                    {
                        // var perWord = (DateTime.Now - start).TotalMilliseconds / 1000;
                        // Console.WriteLine($"init: {perWord:F2}s");
                        start = DateTime.Now;
                    }

                    cnt++;
                    // if (cnt % 10000 == 0)
                    // {
                    //     var perWord = (DateTime.Now - start).TotalMilliseconds / cnt;
                    //     Console.WriteLine($"#{cnt}: {perWord:F2} ms/word; Found+Morphs/Total: {(found+morphs)}/{notFoundList.Count}");
                    // }
                }
            }

            // File.WriteAllLines(notFoundFile, notFoundList);

            var perWord = (DateTime.Now - start).TotalMilliseconds / wordSet.Count * 1000;
            Console.WriteLine($"Total words {wordSet.Count} Found {found} Morphs {morphs} Not found {notFound}; Speed: {perWord:F2} ms/(1000 words)");
        }
    }
}