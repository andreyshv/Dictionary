using System;
using System.Collections.Generic;
using System.Linq;
using Dict.WordNet.Model;

namespace Dict.WordNet.Tools
{
    public class Converter
    {
        /// <summary>
        /// Convert method creates db file from wndb files
        /// dictpath - path to wndb data files
        /// context - dest db context
        /// </summary>
        public static void Convert(string dictPack, WordNetContext context)
        {
            WNDB wndb = new WNDB(dictPack);
            var synWords = new List<string>();
            // int ind;

            var wordToLemma = new Dictionary<string, Lemma>();
            var words = new Dictionary<string, Writing>();

            var poses = (new []{ "n", "v", "a", "r" }).Select(s => PartOfSpeech.of(s));
            foreach (var pos in poses)
            {
                Console.WriteLine("Process Data of {0}", pos.name);
                // ind = 0;

                foreach (var data in wndb.GetData(pos))
                {
                    if (data.pos != pos.symbol && !(data.pos == "s" && pos.symbol == "a")) //data.adj includes 'a' & 's' pos symbols
                        throw new Exception("pos!=data.pos");

                    var synset = new SynSet { Pos = data.pos };
                    context.SynSets.Add(synset);
                    synWords.Clear();

                    foreach (var oword in data.origWords)
                    {
                        Lemma lemma;
                        string lcWord = oword.word.ToLower();

                        // add lemma
                        if (!wordToLemma.TryGetValue(lcWord, out lemma))
                        {
                            lemma = new Lemma { Value = lcWord, Poses = data.pos };
                            wordToLemma.Add(lcWord, lemma);
                            context.Lemmas.Add(lemma);
                        }
                        else if (!lemma.Poses.Contains(data.pos))
                        {
                            lemma.Poses += data.pos;
                        }

                        if (synWords.IndexOf(lcWord) < 0)
                        {
                            synWords.Add(lcWord);

                            // add SynSet <-> Lemma relation
                            context.SynsetLemmas.Add(new SynsetLemma
                            {
                                SynSet = synset,
                                Lemma = lemma
                            });
                        }

                        // add original word if it differs from lemma
                        Writing word;
                        if (lcWord != oword.word)
                        {
                            if (!words.TryGetValue(oword.word, out word))
                            {
                                word = new Writing { Value = oword.word, Lemma = lemma };
                                words.Add(oword.word, word);
                                context.Writings.Add(word);
                            }
                            else if (word.Lemma != lemma)
                            {
                                Console.WriteLine("Word mix: {0} {1} {2}", oword.word, lemma.Value, word.Lemma.Value);
                                continue;
                            }
                        }
                    }

                    synset.Definition = string.Join(";", data.definitions);
                    synset.Example = string.Join(";", data.examples);

                    // ind++;
                    // if (ind % 1000 == 0)
                    //     ShowProgress(ind.ToString());
                }
                Console.WriteLine("Save changes");
                context.SaveChanges();

                // exceptions
                //TODO: remove morphes, ... 

                Console.WriteLine("Process Exceptions of {0}", pos.name);
                // ind = 0;

                foreach (var exwords in GetExceptions(wndb, pos))
                {
                    for (int i = 1; i < exwords.Length; i++)
                    {
                        if (exwords[i] == exwords[0])
                            continue;

                        Lemma lemma;
                        if (wordToLemma.TryGetValue(exwords[i], out lemma)
                            || (exwords[i].Contains('-') && wordToLemma.TryGetValue(exwords[i].Replace('-', ' '), out lemma)))
                        {
                            context.Excepts.Add(new Except { Value = exwords[0], MainForm = exwords[i], Lemma = lemma });
                        }
                        // else
                        // {
                        //     Console.WriteLine("Lemma not found {0}", exwords[i]);
                        //     context.Excepts.Add(new Except { Value = exwords[0], MainForm = exwords[i] });
                        // }
                    }

                    // ind++;
                    // if (ind % 1000 == 0)
                    //     ShowProgress(ind.ToString());
                }
                Console.WriteLine("Save changes");
                context.SaveChanges();
            }

            //Console.WriteLine("Save changes");
            context.SaveChanges();
        }

        private static IEnumerable<string[]> GetExceptions(WNDB wndb, PartOfSpeech pos)
        {
            using (var reader = wndb.GetStreamReader(wndb.ExcFile(pos)))
            {
                reader.BaseStream.Position = 0;
                string prevVal = "";
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == "")
                        continue;

                    var words = line.Split(' ');
                    if (words.Length < 2)
                        throw new Exception("Wrong exception: " + line);
                    if (words[0] == prevVal)
                        throw new Exception("Same exception: " + line);

                    var res = words.Select(w => w.Replace('_', ' ')).Distinct().ToArray();
                    if (res[0] != words[0].Replace('_', ' '))
                        throw new Exception("wrong distinct operation");

                    // if (words.Length > 2)
                    //     Console.WriteLine("ext: " + line);

                    yield return res;
                }
            }
        }
        
        private static int progLine;
        private static void ShowProgress(string s)
        {
            if (Console.CursorLeft == 0 && Console.CursorTop == progLine + 1)
                Console.CursorTop = progLine;
            else
                progLine = Console.CursorTop;

            Console.WriteLine(s);
        }
    }
}
