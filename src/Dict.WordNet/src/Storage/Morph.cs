using System;
using System.Collections.Generic;
using System.Linq;

namespace Dict.WordNet.Json
{
    /// <summary>
    /// WordNet search code morphology functions
    /// </summary>
    public class Morph
    {
        static string[][] sufx =
        {
            /* Noun suffixes */
            new []{ "s", "ses", "xes", "zes", "ches", "shes", "men", "ies" },
            /* Verb suffixes */
            new []{ "s", "ies", "es", "es", "ed", "ed", "ing", "ing" },
            /* Adjective suffixes */
            new []{ "er", "est", "er", "est" }
        };
        static string[][] addr =
        { 
            /* Noun endings */
            new []{ "", "s", "x", "z", "ch", "sh", "man", "y" },
            /* Verb endings */
            new []{ "", "y", "e", "", "e", "", "e", "" },
            /* Adjective endings */
            new []{ "", "", "e", "e" }
        };

        static string[] prepositions =
        {
            "to", "at", "of", "on", "off", "in", "out", "up", "down", "from",
            "with", "into", "for", "about", "between"
        };

        private static IEnumerable<PartOfSpeech> _poses = (new string[] { "noun", "verb", "adj" }).Select(s => PartOfSpeech.of(s));

        private DictStorage _storage;
        public Morph(DictStorage storage)
        {
            _storage = storage;
        }


        public string[] GetMorphs(string str)
        {
            return _poses.SelectMany(p => GetMorphs(str, p))
                .ToArray();
        }

        public IEnumerable<string> GetMorphs(string str, PartOfSpeech pos)
        {
            if (pos.clss == "SATELLITE")
                pos = PartOfSpeech.of("adj");

            var parts = str.Split(' ');
            int cnt = parts.Length;
            string tmp = null;

            /* first try exception list */
            var e = _storage.GetBasicForms(str, pos);
            if (e.Any())
            {
                foreach(var s in e)
                    yield return s;
            }
            else if (pos.name == "verb" && cnt > 1 && HasPreposition(parts))
            {
                yield return MorphPreposition(str, parts, pos);
                yield break;
            }
            else
            /* then try simply morph on original string */
            if (pos.name != "verb" && MorphWord(str, pos, morph => tmp = morph))
            {
                yield return tmp;
            }
            else
            {
                bool isChanged = false;
                for (int i = 0; i < parts.Length; i++)
                {
                    string word = parts[i];
                    if (word.Contains('-'))
                    {
                        bool isSubChanged = false;
                        var subs = word.Split('-');
                        for (int j = 0; j < subs.Length; j++)
                        {
                            isSubChanged |= MorphWord(subs[j], pos, morph => subs[j] = morph);
                        }

                        if (isSubChanged)
                        {
                            parts[i] = string.Join("-", subs);
                            isChanged = true;
                        }
                    }
                    else
                    {
                        isChanged |= MorphWord(word, pos, morph => parts[i] = morph);
                    }
                }

                if (isChanged)
                {
                    var s = string.Join(" ", parts);
                    if (_storage.IsDefinded(s, pos))
                        yield return s;
                }
            }
        }

        public static string GetBasicForm(string word, PartOfSpeech pos) 
        {
            if (pos.name == "adv")
                return "";

            string tmpbuf = word;
            string end = "";
            if (pos.name == "noun")
            {
                if (word.EndsWith("ful"))
                {
                    tmpbuf = word.Substring(0, word.Length - 3);
                    end = "ful";
                }
                else if (word.EndsWith("ss") || word.Length <= 2)
                    return "";
            }

            var psufx = sufx[pos.ident];
            for (int i = 0; i < psufx.Length; i++)
            {
                string suffix = psufx[i];
                if (tmpbuf.EndsWith(suffix))
                {
                    string retval = tmpbuf.Substring(0, tmpbuf.Length - suffix.Length) + addr[pos.ident][i];
                    return retval + end;
                }
            }

            return "";
        }

        private bool MorphWord(string word, PartOfSpeech pos, Action<string> action)
        {
            if (word == null)
                return false;

            var morph = _storage.GetBasicForms(word, pos).FirstOrDefault();
            if (morph != null)
            {
                action(morph);
                return true;
            }

            if (pos.name == "adv")
                return false;

            string tmpbuf = word;
            string end = "";
            if (pos.name == "noun")
            {
                if (word.EndsWith("ful"))
                {
                    tmpbuf = word.Substring(0, word.Length - 3);
                    end = "ful";
                }
                else if (word.EndsWith("ss") || word.Length <= 2)
                    return false;
            }

            var psufx = sufx[pos.ident];
            for (int i = 0; i < psufx.Length; i++)
            {
                string suffix = psufx[i];
                if (tmpbuf.EndsWith(suffix))
                {
                    string retval = tmpbuf.Substring(0, tmpbuf.Length - suffix.Length) + addr[pos.ident][i];
                    if (_storage.IsDefinded(retval, pos))
                    {
                        action(retval + end);
                        return true;
                    }
                }
            }

            return false;
        }

        /* Find a preposition in the verb string */
        private bool HasPreposition(string[] parts)
        {
            for (int i = 1; i <= parts.Length; i++)
            {
                if (prepositions.Contains(parts[i]))
                    return true;
            }
            return false;
        }

        private string MorphPreposition(string str, string[] parts, PartOfSpeech pos)
        {
            string retval;

            /* Assume that the verb is the first word in the phrase.  Strip it
			   off, check for validity, then try various morphs with the
			   rest of the phrase tacked on, trying to find a match. */

            string rest = str.Substring(parts.First().Length);
            string end = null;
            if (parts.Length > 2)
            {   // more than 2 words
                MorphWord(parts.Last(), pos, morph => end = rest.Substring(0, parts.Last().Length) + morph);
            }

            string word = parts.First();
            if (!word.All(c => char.IsLetterOrDigit(c)))
                return null;

            /* First try to find the verb in the exception list */
            var e = _storage.GetBasicForms(word, PartOfSpeech.of("verb"));
            foreach (string excWord in e)
            {
                retval = excWord + rest;
                if (_storage.IsDefinded(retval, PartOfSpeech.of("verb")))
                    return retval;

                if (end != null)
                {
                    retval = excWord + end;
                    if (_storage.IsDefinded(retval, PartOfSpeech.of("verb")))
                        return retval;
                }
            }

            var psufx = sufx[PartOfSpeech.of("verb").ident];
            for (int i = 0; i < psufx.Length; i++)
            {
                string suffix = psufx[i];
                if (word.EndsWith(suffix)) // ending is different
                {
                    string excWord = word.Substring(0, word.Length - suffix.Length) + addr[PartOfSpeech.of("verb").ident][i];

                    retval = excWord + rest;
                    if (_storage.IsDefinded(retval, PartOfSpeech.of("verb")))
                        return retval;

                    if (end != null)
                    {
                        retval = excWord + end;
                        if (_storage.IsDefinded(retval, PartOfSpeech.of("verb")))
                            return retval;
                    }
                }
            }

            if (end != null)
            {
                return word + end;
            }

            return null;
        }
    }
}
