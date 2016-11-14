using System.Linq;

namespace Dict.WordNet.Json
{
    public class Search
    {
        private DictStorage _storage;
        private string[] _morphStrings;
        private SynSetGroup[] _result;

        public string Word { get; private set; }

        public Search(string word, DictStorage storage)
        {
            Word = word;
            _storage = storage;
        }

        /// <summary>
        /// Search wordnet database for word and return found synsets 
        /// </summary>
        public SynSetGroup[] SynSetGroups
        {
            get
            {
                if (_result == null)
                {
                    var str = Word.ToLower();
                    if (str.Intersect(" -.").Any())
                    {
                        var strings = new string[]
                        {
                            str,
                            str.Replace(' ', '-'),
                            str.Replace('-', ' '),
                            str.Replace("-", ""),
                            str.Replace(".", "")
                        };

                        _result = strings
                            .Distinct()
                            .SelectMany(s =>
                            {
                                SynSetGroup[] grp;
                                return (_storage.SynSets.TryGetValue(s, out grp) ? grp : Enumerable.Empty<SynSetGroup>());
                            })
                            .ToArray();
                    }
                    else if (!_storage.SynSets.TryGetValue(str, out _result))
                    {
                        _result = new SynSetGroup[0];
                    }

                }

                return _result;
            }
        }

        /// <summary>
        /// Fill the morphlist - eg. if verb relations of 'drunk' are requested, none are directly 
        /// found, but the morph 'drink' will have results.  The morph hashtable will be populated 
        /// into the search results and should be iterated instead of the returned synset if the 
        /// morphs are non-empty
        /// </summary>
        public string[] MorphStrings
        {
            get
            {
                if (_morphStrings == null)
                {
                    var morph = new Morph(_storage);
                    _morphStrings = morph.GetMorphs(Word)
                        .ToArray();
                }

                return _morphStrings;
            }
        }
    }
}