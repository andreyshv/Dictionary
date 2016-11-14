using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Dict.WordNet.Json
{
    [JsonObject(IsReference = true)]
    public class SynSet
    {
        // ASCII form of a word as entered in the synset by the lexicographer. The text of the word is case sensitive.
        public string[] Synonims { get; set; }

        public string Definition { get; set; }
        public string[] Definitions { get; set; }

        public string Example { get; set; }
        public string[] Examples { get; set; }
    }

    [JsonObject(IsReference = true)]
    public class SynSetGroup
    {
        public char PosSymbol { get; set; }
        public SynSet[] Synsets { get; set; }
    }

    public class DictException
    {
        public string BasicForm { get; set; }
        public string PosSymbols { get; set; }
    }

    public class DictStorage
    {
        private List<string> _wordList;

        public ReadOnlyDictionary<string, SynSetGroup[]> SynSets { get; set; }
        public ReadOnlyDictionary<string, DictException[]> Exceptions { get; set; }

        public static DictStorage Open(string fileName)
        {
            var serializer = new JsonSerializer();

            using (var stream = File.OpenRead(fileName))
            using (var reader = new BsonReader(stream))
            {
                return serializer.Deserialize<DictStorage>(reader);
            }
        }

        [OnDeserialized]
        internal void OnSerializedMethod(StreamingContext context)
        {
            _wordList = SynSets.Keys.ToList();
            _wordList.Sort();
        }
        
        public bool IsDefinded(string word, PartOfSpeech pos)
        {
            SynSetGroup[] synSetGrp;
            return SynSets.TryGetValue(word, out synSetGrp) && synSetGrp.Any(grp => grp.PosSymbol == pos.symbol.First());
        }

        public IEnumerable<string> GetBasicForms(string word)
        {
            DictException[] excepts;
            if (Exceptions.TryGetValue(word, out excepts))
            {
                return excepts.AsEnumerable().Select(e => e.BasicForm);
            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetBasicForms(string word, PartOfSpeech pos)
        {
            DictException[] excepts;
            if (Exceptions.TryGetValue(word, out excepts))
            {
                return excepts.Where(e => e.PosSymbols.Contains(pos.symbol)).Select(e => e.BasicForm);
            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<DictException> GetExceptions(string word)
        {
            DictException[] excepts;
            if (Exceptions.TryGetValue(word, out excepts))
            {
                return excepts.AsEnumerable();
            }

            return Enumerable.Empty<DictException>();
        }

        // Lookup searches strings which starts with part  
        public IEnumerable<string> Lookup(string part)
        {
            int index = _wordList.BinarySearch(part);
            if (index < 0)
                index = ~index;
            else if (index > 0)
                index--;
                
            foreach (var w in _wordList.Skip(index).Where(s => s.StartsWith(part)))
                yield return w;
        }
    }
}