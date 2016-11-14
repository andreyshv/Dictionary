using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Dict.WordNet.Json;
using DictionaryExtentions;
using Newtonsoft.Json.Linq;

namespace Dict.WordNet.Tools
{
    // BSON converter
    public class JsonConverter
    {
        /// <summary>
        /// Convert method creates db file from wndb files
        /// dictpath - path to wndb data files
        /// context - dest db context
        /// </summary>
        public static void Convert(string dictPack, string jsonFile)
        {
            WNDB wndb = new WNDB(dictPack);
            var poses = (new[] { "n", "v", "a", "r" }).Select(s => PartOfSpeech.of(s));

            // Convert to  Dictionary
            // lemma -> { SynSetGroup: PosSymbol, Synsets = { synset: synonims, definitions, examples } }

            var dict = new Dictionary<string, List<ExpSynSetGroup>>();
            foreach (var pos in poses)
            {
                Console.WriteLine("Process Data of {0}", pos.name);

                foreach (var data in wndb.GetData(pos))
                {
                    //data.adj includes 'a' & 's' pos symbols
                    char posSymbol = pos.symbol.First();

                    bool singleWord = false;
                    if (data.origWords.Count() == 1)
                    {
                        var w = data.origWords.First().word;
                        singleWord = w == w.ToLower();
                    }

                    var synSet = new SynSet
                    {
                        // Skip synonims if where is a single lowercase word  
                        Synonims = (singleWord) ? null : data.origWords.Select(ow => ow.word).ToArray(),
                        Definition = (data.definitions.Count() == 1) ? data.definitions.First() : null,
                        Definitions = (data.definitions.Count() > 1) ? data.definitions : null,
                        Example = (data.examples?.Count() == 1) ? data.examples.First() : null, 
                        Examples = (data.examples?.Count() > 1) ? data.examples : null
                    };

                    foreach (var lemma in data.origWords.Select(ow => ow.word.ToLower()))
                    {
                        var synGrps = dict.GetValue(lemma); 
                        if (synGrps != null)
                        {
                            var grp = synGrps.FirstOrDefault(g => g.PosSymbol == posSymbol);

                            if (grp == null)
                            {
                                synGrps.Add(new ExpSynSetGroup(posSymbol, synSet));
                            }
                            else
                            {
                                grp.Synsets.Add(synSet);
                            }
                        }
                        else
                        {
                            dict.Add(lemma, new List<ExpSynSetGroup> { new ExpSynSetGroup(posSymbol, synSet) });
                        }
                    }
                }
            }

            // exceptions
            //TODO: remove morphes, ...

            var excepts = new Dictionary<string, List<DictException>>();
            foreach (var pos in poses)
            {
                Console.WriteLine("Process Exceptions of {0}", pos.name);

                foreach (var exwords in wndb.GetExceptions(pos))
                {
                    var morph = Morph.GetBasicForm(exwords[0], pos);
                    for (int i = 1; i < exwords.Length; i++)
                    {
                        var baseForm = exwords[i]; 
                        if (baseForm == exwords[0] || baseForm == morph)
                        {
                            //Console.WriteLine($"Skip: {(exwords[0])} -> {baseForm}/{morph}");
                            continue;
                        }

                        List<ExpSynSetGroup> synGrps = dict.GetValue(baseForm);
                        if (synGrps == null && baseForm.Contains('-'))
                        {
                            baseForm = baseForm.Replace('-', ' ');
                            dict.TryGetValue(baseForm, out synGrps);
                        }

                        if (synGrps != null)
                        {
                            var posSymbols = string.Join("", synGrps.Select(sg => sg.PosSymbol));
                            var except = new DictException { BasicForm = baseForm, PosSymbols = posSymbols };

                            List<DictException> baseForms;
                            if (excepts.TryGetValue(exwords[0], out baseForms))
                            {
                                if (!baseForms.Any(e => e.BasicForm == baseForm))
                                    baseForms.Add(except);
                            }
                            else
                            {
                                excepts.Add(exwords[0], new List<DictException> { except });
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Save changes");
            
            var storage = new ExpDictStorage 
            {
                SynSets = dict,
                Exceptions = excepts
            };

            var serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            
            using (var stream = File.Open(jsonFile, FileMode.Create))
            using (var writer = new BsonWriter(stream))
            {
                serializer.Serialize(writer, storage);
            }
        }

        public void CopyBsonToJson(string fileName)
        {
            JToken tok;
            using (var stream = File.OpenRead(fileName))
            {
                tok = JToken.ReadFrom(new BsonReader(stream));
            }

            var jsonFile = Path.GetFileNameWithoutExtension(fileName) + ".json";
            using (var stream = File.Open(jsonFile, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(stream))
            {
                sw.Write(tok.ToString());
            }
        }

        [JsonObject(IsReference = true)]
        private class ExpSynSetGroup
        {
            public char PosSymbol { get; set; }
            public List<SynSet> Synsets { get; set; }

            public ExpSynSetGroup(char posSymbol, SynSet synSet)
            {
                PosSymbol = posSymbol;
                Synsets = new List<SynSet>() { synSet };
            }
        }    
        
        private class ExpDictStorage
        {
            public Dictionary<string, List<ExpSynSetGroup>> SynSets { get; set; }
            public Dictionary<string, List<DictException>> Exceptions { get; set; }
        }
    }
}