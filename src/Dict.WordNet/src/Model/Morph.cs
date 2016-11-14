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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Dict.WordNet.Model
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

        public static IEnumerable<string> GetMorphs(string str, PartOfSpeech pos, WordNetContext context)
        {
            if (pos == null)
            {
                var poses = (new string[] { "n", "v", "a" }).Select(s => PartOfSpeech.of(s));
                foreach(var p in poses)
                {
                    foreach (var s in GetMorphs(str, p, context))
                    {
                        yield return s;
                    }
                }
                yield break;
            }

            if (pos.clss == "SATELLITE")
                pos = PartOfSpeech.of("adj");

            var parts = str.Split(' ');
            int cnt = parts.Length;
            string tmp = null;

            /* first try exception list */
            var e = GetExceptions(str, pos, context);
            if (e.Any())
            {
                foreach(var s in e)
                    yield return s;
            }
            else if (pos.name == "verb" && cnt > 1 && HasPreposition(parts))
            {
                yield return MorphPreposition(str, parts, pos, context);
                yield break;
            }
            else
            /* then try simply morph on original string */
            if (pos.name != "verb" && MorphWord(str, pos, morph => tmp = morph, context))
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
                            isSubChanged |= MorphWord(subs[j], pos, morph => subs[j] = morph, context);
                        }

                        if (isSubChanged)
                        {
                            parts[i] = string.Join("-", subs);
                            isChanged = true;
                        }
                    }
                    else
                    {
                        isChanged |= MorphWord(word, pos, morph => parts[i] = morph, context);
                    }
                }

                if (isChanged)
                {
                    var s = string.Join(" ", parts);
                    if (IsDefinded(s, pos, context))
                        yield return s;
                }
            }
        }

        private static bool MorphWord(string word, PartOfSpeech pos, Action<string> action, WordNetContext context)
        {
            if (word == null)
                return false;

            var morph = GetExceptions(word, pos, context).FirstOrDefault();
            if (morph != null)
            {
                action(morph);
                return true;
            }

            if (pos.name == "adverb")
                return false;

            string tmpbuf = "";
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

            if (tmpbuf == "")
                tmpbuf = word;

            var psufx = sufx[pos.ident];
            for (int i = 0; i < psufx.Length; i++)
            {
                string suffix = psufx[i];
                if (tmpbuf.EndsWith(suffix))
                {
                    string retval = tmpbuf.Substring(0, tmpbuf.Length - suffix.Length) + addr[pos.ident][i];
                    if (IsDefinded(retval, pos, context))
                    {
                        action(retval + end);
                        return true;
                    }
                }
            }

            return false;
        }

        /* Find a preposition in the verb string */
        private static bool HasPreposition(string[] parts)
        {
            for (int i = 1; i <= parts.Length; i++)
            {
                if (prepositions.Contains(parts[i]))
                    return true;
            }
            return false;
        }

        private static string MorphPreposition(string str, string[] parts, PartOfSpeech pos, WordNetContext context)
        {
            string retval;

            /* Assume that the verb is the first word in the phrase.  Strip it
			   off, check for validity, then try various morphs with the
			   rest of the phrase tacked on, trying to find a match. */

            string rest = str.Substring(parts.First().Length);
            string end = null;
            if (parts.Length > 2)
            {   // more than 2 words
                MorphWord(parts.Last(), pos, morph => end = rest.Substring(0, parts.Last().Length) + morph, context);
            }

            string word = parts.First();
            if (!word.All(c => char.IsLetterOrDigit(c)))
                return null;

            /* First try to find the verb in the exception list */
            var e = GetExceptions(word, PartOfSpeech.of("verb"), context);
            foreach (string excWord in e)
            {
                retval = excWord + rest;
                if (IsDefinded(retval, PartOfSpeech.of("verb"), context))
                    return retval;

                if (end != null)
                {
                    retval = excWord + end;
                    if (IsDefinded(retval, PartOfSpeech.of("verb"), context))
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
                    if (IsDefinded(retval, PartOfSpeech.of("verb"), context))
                        return retval;

                    if (end != null)
                    {
                        retval = excWord + end;
                        if (IsDefinded(retval, PartOfSpeech.of("verb"), context))
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

        private static bool IsDefinded(string word, PartOfSpeech pos, WordNetContext context)
        {
            return context.IsDefinded(word, pos);
        }

        private static Dictionary<string, List<DictException>> _exceptCache;
        // extracted from Exceptions class 
        private static IEnumerable<string> GetExceptions(string word, PartOfSpeech pos, WordNetContext context)
        {
            if (_exceptCache == null)
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();

                _exceptCache = context.Excepts
                    .Include(e => e.Lemma)
                    .GroupBy(e => e.Value, e => e.Lemma)
                    .ToDictionary(grp => grp.Key, grp => grp.Select(l => new DictException { Exception = l.Value, Poses = l.Poses }).ToList());

                watch.Stop();
                Console.WriteLine($"GetExceptions time: {watch.ElapsedMilliseconds} ms");
            }

            List<DictException> excepts;
            if (_exceptCache.TryGetValue(word, out excepts))
            { 
                return excepts.Where(e => e.Poses.Contains(pos.symbol))
                    .Select(e => e.Exception);
            }
            
            return Enumerable.Empty<string>();
        }
    }

    internal class DictException
    {
        public string Exception { get; set; }
        public string Poses { get; set; }
    }
}
