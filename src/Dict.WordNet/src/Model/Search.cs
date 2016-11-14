using System.Linq;

namespace Dict.WordNet.Model
{
    public class Search
    {
        private WordNetContext _context;

        public string word { get; private set; }
        public PartOfSpeech pos { get; private set; }
        public string[] MorphStrings { get; private set; }
        public Search[] MorphSearches { get; private set; }
        public PosGlosses[] SynSets { get; private set; }

        public static Search GetSearch(string w, WordNetContext context)
        {
            bool doMorphs = true;
            PartOfSpeech p = null;

            var search = new Search { word = w, pos = p, _context = context };
            //if (p != null)
            search.do_search(doMorphs);

            return search;
        }

        public static Search GetSearch(string w, bool doMorphs, PartOfSpeech p, WordNetContext context)
        {
            var search = new Search { word = w, pos = p, _context = context };
            //if (p != null)
            search.do_search(doMorphs);

            return search;
        }

        private Search()
        {
        }

        private PosGlosses[] GetIndexesQuery(string str) // , PartOfSpeech pos
        {
            str = str.ToLower();
            var strings = new string[]
            {
                str,
                str.Replace(' ', '-'),
                str.Replace('-', ' '),
                str.Replace("-", ""),
                str.Replace(".", "")
            };            

            PosGlosses[] defs;
            return strings.Distinct()
                .SelectMany(s => _context.Defs.TryGetValue(s, out defs) ? defs : Enumerable.Empty<PosGlosses>())
                .ToArray();
        }

        private void do_search(bool doMorphs)
        {
            SynSets = GetIndexesQuery(word);

            if (SynSets.Length == 0 && doMorphs)
            {
                // Fill the morphlist - eg. if verb relations of 'drunk' are requested, none are directly 
                // found, but the morph 'drink' will have results.  The morph hashtable will be populated 
                // into the search results and should be iterated instead of the returned synset if the 
                // morphs are non-empty

                MorphStrings = Morph.GetMorphs(word, pos, _context)
                    .ToArray();

                // MorphSearches = MorphStrings
                //     .Select(str => GetSearch(str, false, pos, _context))
                //     .ToArray();
            }
        }
    }
}