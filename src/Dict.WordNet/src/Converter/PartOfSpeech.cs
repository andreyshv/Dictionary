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

namespace Dict.WordNet
{
    //TODO: [Serializable]
    public class PartOfSpeech
    {
        //TODO: [NonSerialized()]
        public static IDictionary<string, PartOfSpeech> Parts { get; private set; }

        public string symbol { get; private set; }
        public string name { get; private set; }
        public string clss { get; private set; }
        public int ident { get; private set; }
        public PartsOfSpeech flag { get; private set; }

        public string key
        {
            get
            {
                return name;
            }
        }

        static int uniq = 0;
        //internal Dictionary<> help = new Dictionary<>(); // string searchtype->string help: see WnHelp

        static PartOfSpeech()
        {
            Parts = new Dictionary<string, PartOfSpeech>();

            new PartOfSpeech("n", "noun", PartsOfSpeech.Noun);  // 0
            new PartOfSpeech("v", "verb", PartsOfSpeech.Verb);  // 1
            new PartOfSpeech("a", "adj", PartsOfSpeech.Adj);    // 2
            new PartOfSpeech("r", "adv", PartsOfSpeech.Adv);    // 3
            new PartOfSpeech("s", "adj", "SATELLITE", PartsOfSpeech.Adj);
        }

        private PartOfSpeech()
        {
            // empty constructor for serialization
        }

        PartOfSpeech(string s, string n, string c, PartsOfSpeech f)
        {
            symbol = s;
            name = n;
            clss = c;
            flag = f;
            ident = uniq++;
            Parts[s] = this;
            if (c == "") // exclude SATELLITE
                Parts[name] = this;
        }

        PartOfSpeech(string s, string n, PartsOfSpeech f)
            : this(s, n, "", f)
        {
        }

        public static PartOfSpeech of(string s)
        {
            return Parts[s];
        }

        public static PartOfSpeech of(PartsOfSpeech f)
        {
            switch (f)
            {
                case PartsOfSpeech.Noun:
                    return of("noun");
                case PartsOfSpeech.Verb:
                    return of("verb");
                case PartsOfSpeech.Adj:
                    return of("adj");
                case PartsOfSpeech.Adv:
                    return of("adv");
            }

            return null;            // unknown or not unique
        }

        public override string ToString()
        {
            return name;
        }
    }

    [Flags]
    public enum PartsOfSpeech
    {
        Unknown = 0,
        Noun = 1,
        Verb = 2,
        Adj = 4,
        Adv = 8
    }

}
