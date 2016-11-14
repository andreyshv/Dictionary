/*
 * This file is a part of the WordNet.Net open source project.
 * 
 * Copyright (C) 2005 Malcolm Crowe, Troy Simpson
 * 
 * Project Home: http://www.ebswift.com
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * 
 * */

// util.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Dict.WordNet.Tools
{
    public class WNDB
    {
        private ZipArchive _arc; 
        private Dictionary<string, StreamReader> _indexFps = new Dictionary<string, StreamReader>();
        private Dictionary<string, StreamReader> _dataFps = new Dictionary<string, StreamReader>();

        public WNDB(string dbPack)
        {
            _arc = ZipFile.OpenRead(dbPack);

            foreach (var part in PartOfSpeech.Parts.Values)
            {
                if (!_indexFps.ContainsKey(part.key))
                    _indexFps[part.key] = GetStreamReader(IndexFile(part));
                if (!_dataFps.ContainsKey(part.key))
                    _dataFps[part.key] = GetStreamReader(DataFile(part));
            }
        }

        /// Read data file to DataItem's list
        public IEnumerable<DataItem> GetData(PartOfSpeech pos)
        {
            using (var reader = data(pos))
            {
                var defs = new List<string>();
                var exams = new List<string>();

                reader.BaseStream.Position = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith(" "))
                        continue;

                    var data = new DataItem();
                    StrTok st = new StrTok(line);

                    data.offset = int.Parse(st.next());
                    data.lex_filenum = int.Parse(st.next());
                    data.pos = st.next();
                    if (data.pos == "s") //(pos.clss == "SATELLITE")
                        data.sstype = AdjSynSetType.IndirectAnt;

                    int w_cnt = int.Parse(st.next(), NumberStyles.HexNumber);
                    data.origWords = new OrigWord[w_cnt];
                    for (var i = 0; i < w_cnt; i++)
                    {
                        var origWord = new OrigWord { word = st.next().Replace('_', ' ') };
                        // syntactic marker 
                        if (pos.name == "adj")
                        {
                            var ind = origWord.word.IndexOf('(');
                            if (ind > 0)
                            {
                                origWord.marker = origWord.word.Substring(ind); 
                                origWord.word = origWord.word.Substring(0, ind);
                            }
                        }

                        origWord.lex_id = int.Parse(st.next(), NumberStyles.HexNumber); // uni
                        data.origWords[i] = origWord;

                        // missing word sense values
                        //words[i].wnsns = getsearchsense(i);

                        //if (words[i].word.ToLower() == word)
                        //    whichword = j + 1;
                    }

                    int p_cnt = int.Parse(st.next());
                    data.pointers = new SynPointer[p_cnt];
                    for (int i = 0; i < p_cnt; i++)
                    {
                        data.pointers[i] = new SynPointer { pointer_symbol = st.next() };
                        if (data.pos == "a" && data.sstype == AdjSynSetType.DontKnow)
                        {
                            if (data.pointers[i].pointer_symbol == "!") //.ptp.ident == ANTPTR) // TDMS 11 JUL 2006 - change comparison to int //.mnemonic=="ANTPTR")
                                data.sstype = AdjSynSetType.DirectAnt;
                            else if (data.pointers[i].pointer_symbol == @"\") //.ptp.ident == PERTPTR) // TDMS 11 JUL 2006 - change comparison to int //mnemonic=="PERTPTR")
                                data.sstype = AdjSynSetType.Pertainym;
                        }

                        data.pointers[i].synset_offset = int.Parse(st.next());
                        data.pointers[i].pos = st.next();
                        int sd = int.Parse(st.next(), NumberStyles.HexNumber);
                        data.pointers[i].source = sd >> 8;
                        data.pointers[i].dest = sd & 0xff;
                    }

                    // In data.verb only
                    string f = st.next();
                    if (f != "|")
                    {
                        var frames = new List<SentFrame>();
                        int f_cnt = int.Parse(f);
                        for (int i = 0; i < f_cnt; i++)
                        {
                            f = st.next(); // +
                            var frame = new SentFrame { f_num = int.Parse(st.next()) };
                            frame.w_num = int.Parse(st.next(), NumberStyles.HexNumber);
                            frames.Add(frame);
                        }
                        data.frames = frames.ToArray();
                        f = st.next();
                    }

                    // Parse gloss

                    var gloss = line.Substring(line.IndexOf('|') + 1).TrimEnd();
                    if (gloss != "" && gloss.Last() != ';')
                        gloss += ";";

                    bool first = true;
                    bool inQuotes = false;
                    bool example = false;
                    var sb = new StringBuilder();
                    foreach (var c in gloss)
                    {
                        if (first && c == ' ')
                        {
                            // skip leading spaces
                        }
                        else if ((first || inQuotes) && c == '"')
                        {
                            inQuotes = !inQuotes;
                            example |= inQuotes;
                            first = false;
                            sb.Append(c);
                        }
                        else if (!inQuotes && c == ';')
                        {
                            var s = sb.ToString().TrimEnd().Replace("--", "-");

                            if (example) 
                            {
                                // remove quotes if no author written 
                                if (s.EndsWith("\""))
                                    s = s.Substring(1, s.Length - 2);

                                exams.Add(s);
                            }
                            else
                            {    
                                defs.Add(s);
                            }

                            sb.Clear();
                            first = true;
                            example = false;
                        }
                        else 
                        {
                            sb.Append(c);
                            first = false;
                        }
                    }
                    
                    data.examples = exams.Any() ? exams.ToArray() : null;
                    exams.Clear();
                    data.definitions = defs.ToArray();
                    defs.Clear();

                    yield return data;
                }
            }
        }

        public IEnumerable<string[]> GetExceptions(PartOfSpeech pos)
        {
            using (var reader = GetStreamReader(ExcFile(pos)))
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

        public StreamReader GetStreamReader(string fileName)
        {
            var entry = _arc.GetEntry(fileName);
            if (entry == null)
                return null;

            MemoryStream ms = new MemoryStream();
            using (var stream = entry.Open())
            {
                stream.CopyTo(ms);
            }

            return new StreamReader(ms, Encoding.ASCII, false, 128);
        }

        public StreamReader index(PartOfSpeech p)
        {
            return _indexFps[p.key];
        }

        public StreamReader data(PartOfSpeech p)
        {
            return _dataFps[p.key];
        }

        public void reopen(PartOfSpeech p)
        {
            _indexFps[p.key].Dispose();
            _indexFps[p.key] = GetStreamReader(IndexFile(p));
            _dataFps[p.key].Dispose();
            _dataFps[p.key] = GetStreamReader(DataFile(p));
        }

        public static string[] lexfiles =
        {
            "adj.all",			    /* 0 */
			"adj.pert",			    /* 1 */
			"adv.all",			    /* 2 */
			"noun.Tops",		    /* 3 */
			"noun.act",			    /* 4 */
			"noun.animal",		    /* 5 */
			"noun.artifact",		/* 6 */
			"noun.attribute",		/* 7 */
			"noun.body",		    /* 8 */
			"noun.cognition",		/* 9 */
			"noun.communication",   /* 10 */
			"noun.event",		    /* 11 */
			"noun.feeling",		    /* 12 */
			"noun.food",		    /* 13 */
			"noun.group",		    /* 14 */
			"noun.location",		/* 15 */
			"noun.motive",		    /* 16 */
			"noun.object",		    /* 17 */
			"noun.person",		    /* 18 */
			"noun.phenomenon",		/* 19 */
			"noun.plant",		    /* 20 */
			"noun.possession",		/* 21 */
			"noun.process",		    /* 22 */
			"noun.quantity",		/* 23 */
			"noun.relation",		/* 24 */
			"noun.shape",		    /* 25 */
			"noun.state",		    /* 26 */
			"noun.substance",		/* 27 */
			"noun.time",		    /* 28 */
			"verb.body",		    /* 29 */
			"verb.change",		    /* 30 */
			"verb.cognition",		/* 31 */
			"verb.communication",	/* 32 */
			"verb.competition",		/* 33 */
			"verb.consumption",		/* 34 */
			"verb.contact",		    /* 35 */
			"verb.creation",		/* 36 */
			"verb.emotion",		    /* 37 */
			"verb.motion",		    /* 38 */
			"verb.perception",		/* 39 */
			"verb.possession",		/* 40 */
			"verb.social",		    /* 41 */
			"verb.stative",		    /* 42 */
			"verb.weather",		    /* 43 */
			"adj.ppl",			    /* 44 */
		};

        internal string ExcFile(PartOfSpeech n)
        {
            return n.name + ".exc";
        }

        internal string IndexFile(string name)
        {
            return "index." + name; // WN2.1 - TDMS
        }

        internal string IndexFile(PartOfSpeech n)
        {
            return IndexFile(n.name); // WN2.1 - TDMS
        }

        internal string DataFile(PartOfSpeech n)
        {
            return "data." + n.name; // WN2.1 - TDMS
        }

        public string binSearch(string searchKey, char marker, StreamReader fp)
        {
            long bot = fp.BaseStream.Seek(0, SeekOrigin.End);
            long top = 0;
            long mid = (bot - top) / 2 + top;
            long diff = 666; // ???
            string key = "";
            string line = "";

            do
            {
                fp.DiscardBufferedData();
                fp.BaseStream.Position = mid - 1;

                if (mid != 1)
                    fp.ReadLine();

                line = fp.ReadLine();
                if (line == null) // || line == "")
                    return null;
                line = line.Replace("\0", "");
                int n = Math.Max(line.IndexOf(marker), 0);
                key = line.Substring(0, n);

                int co = string.CompareOrdinal(key, searchKey);
                if (co < 0)
                {
                    // key is alphabetically less than the search key
                    top = mid;
                    diff = (bot - top) / 2;
                    mid = top + diff;
                }
                if (co > 0)
                {
                    // key is alphabetically greater than the search key
                    bot = mid;
                    diff = (bot - top) / 2;
                    mid = top + diff;
                }
            }
            while (key != searchKey && diff != 0);

            if (key == searchKey)
                return line;

            return null;
        }

        public string binSearch(string searchKey, StreamReader fp)
        {
            return binSearch(searchKey, ' ', fp);
        }

        public string binSearch(string word, PartOfSpeech pos)
        {
            return binSearch(word, index(pos));
        }

        public string binSearchSemCor(string uniqueid, string searchKey, StreamReader fp)
        {
            int n;
            searchKey = searchKey.ToLower(); // for some reason some WordNet words are stored with a capitalised first letter, whilst all words in the sense index are lowercase
            string key = "";
            string line = binSearch(searchKey, '%', fp);

            // we have found an exact match (or no match)
            if (line == null || line.IndexOf(uniqueid, 0) > 0)
                return line;

            // set the search down the list and work up
            fp.DiscardBufferedData();
            fp.BaseStream.Position -= 4000;
            //fp.BaseStream.Seek((long)(-1000), SeekOrigin.Current);

            // move down until we find the first matching key
            do
            {
                line = fp.ReadLine();
                if (line == null)
                    return null;
                n = Math.Max(line.IndexOf('%'), 0);
                key = line.Substring(0, n);
            }
            while (key != searchKey);

            // scroll through matching words until the exact identifier is found
            do
            {
                if (line.IndexOf(uniqueid, 0) > 0)
                    return line;

                line = fp.ReadLine();
                if (line == null)
                    return null;
                n = Math.Max(line.IndexOf('%'), 0);
                key = line.Substring(0, n);
            }
            while (key == searchKey);

            return null;
        }

        // TDMS 16 July 2006 - removed this method.
        // Method removed because if called externally
        // WNDBPart was not correctly constructed.
        // Calling is_defined(string searchstr,PartOfSpeech fpos)
        // correctly constructs WNDBPart.
        /*
				private static SearchSet is_defined(string word,string p)
				{
					Console.WriteLine("is_defined string, string");
					return is_defined(word,PartOfSpeech.of(p));
				}
		*/

        /// <summary>
        /// Determines if a word is defined in the WordNet database and returns
        /// all possible searches of the word.
        /// </summary>
        /// <example> This sample displays a message stating whether the 
        /// word "car" exists as the part of speech "noun".
        /// <code>
        /// Wnlib.WNCommon.path = "C:\Program Files\WordNet\2.1\dict\"
        /// Dim wrd As String = "car"
        /// Dim POS As String = "noun"
        /// Dim b As Boolean = Wnlib.WNDB.is_defined(wrd, Wnlib.PartOfSpeech.of(POS)).NonEmpty.ToString
        /// 
        /// If b Then
        /// 	MessageBox.Show("The word " &amp; wrd &amp; " exists as a " &amp; POS &amp; ".")
        /// Else
        /// 	MessageBox.Show("The word " &amp; wrd &amp; " does not exist as a " &amp; POS &amp; ".")
        /// End If
        /// </code>
        /// </example>
        /// <param name="searchstr">The word to search for</param>
        /// <param name="fpos">Part of Speech (noun, verb, adjective, adverb)</param>
        /// <returns>A SearchSet or null if the word does not exist in the dictionary</returns>
        // public SearchSet is_defined(string searchstr, PartOfSpeech fpos)
        // {
        //     Indexes ixs = new Indexes(searchstr, fpos, this);
        //     int i;
        //     int CLASS = 22; /* - */
        //     int LASTTYPE = CLASS;

        //     Search s = new Search(searchstr, fpos, new SearchType(false, "FREQ"), 0, this);
        //     SearchSet retval = new SearchSet();
        //     foreach (Index index in ixs)
        //     {
        //         retval = retval + "SIMPTR" + "FREQ" + "SYNS" + "WNGREP" + "OVERVIEW"; // added WNGREP - TDMS
        //         for (i = 0; i < index.ptruse.Length; i++)
        //         {
        //             PointerType pt = index.ptruse[i];
        //             //			retval=retval+pt;

        //             // WN2.1 - TDMS
        //             if (pt.ident <= LASTTYPE)
        //             {
        //                 retval = retval + pt;
        //             }
        //             else if (pt.mnemonic == "INSTANCE")
        //             {
        //                 retval = retval + "HYPERPTR";
        //             }
        //             else if (pt.mnemonic == "INSTANCES")
        //             {
        //                 retval = retval + "HYPOPTR";
        //             }

        //             // WN2.1 - TDMS
        //             if (pt.mnemonic == "SIMPTR")
        //             {
        //                 retval = retval + "ANTPTR";
        //             }

        //             if (fpos.name == "noun")
        //             {
        //                 /* set generic HOLONYM and/or MERONYM bit if necessary */
        //                 if (pt >= "ISMEMBERPTR" && pt <= "ISPARTPTR")
        //                     retval = retval + "HOLONYM";
        //                 else if (pt >= "HASMEMBERPTR" && pt <= "HASPARTPTR")
        //                     retval = retval + "MERONYM";
        //             }
        //             // WN2.1 - TDMS					else if (fpos.name=="adj" && pt.mnemonic=="SIMPTR")
        //             //						retval=retval+"ANTPTR";
        //         }

        //         if (fpos.name == "noun")
        //         {
        //             retval = retval + "RELATIVES";
        //             if (index.HasHoloMero("HMERONYM", s))
        //                 retval = retval + "HMERONYM";
        //             if (index.HasHoloMero("HHOLONYM", s))
        //                 retval = retval + "HHOLONYM";
        //             if (retval["HYPERPTR"])
        //                 retval = retval + "COORDS";
        //         }
        //         else if (fpos.name == "verb")
        //             retval = retval + "RELATIVES" + "FRAMES"; // added frames - TDMS
        //     }
        //     return retval;
        // }

        internal List<string> wngrep(string wordPassed, PartOfSpeech pos)
        {
            var r = new List<string>();
            StreamReader fp = index(pos);
            fp.BaseStream.Position = 0;
            fp.DiscardBufferedData();
            string word = wordPassed.Replace(" ", "_");
            string line;

            while ((line = fp.ReadLine()) != null)
            {
                int lineLen = line.IndexOf(' ');
                line = line.Substring(0, lineLen);
                try
                {
                    if (line.IndexOf(word) >= 0)
                        r.Add(line.Replace("_", " "));
                }
                catch
                {
                }
            }
            return r;
        }
    }
}
