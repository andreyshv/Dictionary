namespace Dict.WordNet.Tools
{
    public class OrigWord
    {
        public string word;
        public int lex_id;
        public string marker;
    }

    public class SynPointer
    {
        public string pointer_symbol;
        public int synset_offset;
        public string pos;
        public int source;
        public int dest;
    }

    public class SentFrame
    {
        public int f_num;
        public int w_num;
    }

    public enum AdjSynSetType
	{
		DontKnow,
		DirectAnt,
		IndirectAnt,
		Pertainym
	}

    public class DataItem
    {
        public int offset;
        public int lex_filenum;         // lexicographer file index in WNDB.lexfiles[]
        public string pos;              // part of speech = ss_type
        public OrigWord[] origWords;    // words in synset
        public SynPointer[] pointers;   // pointers
        public SentFrame[] frames;
        public string[] definitions;        // synset gloss (definition and/or example(s))
        public string[] examples;

        public AdjSynSetType sstype;

        //public int getsearchsense(int which)
        //{
        //    string wdbuf = words[which].word.ToLower();
        //    Index idx = Index.lookup(wdbuf, pos, _db);
        //    if (idx != null)
        //        for (int i = 0; i < idx.offs.Length; i++)
        //            if (idx.offs[i] == hereiam)
        //                return i + 1;
        //    return 0;
        //}
    }
}
