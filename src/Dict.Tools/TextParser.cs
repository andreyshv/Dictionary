using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dict.Tools
{
    public class TextParser
    {
        public static void ParseFiles(string srcDir, string dstFile)
        {
            // var culture = new CultureInfo("en-US");
            // var comparer = StringComparer.Create(culture, true);

            var wordSet = new HashSet<string>();
            foreach (var fileName in Directory.GetFiles(srcDir, "*.txt"))
            {
                Console.WriteLine("Parse {0}", fileName);
                var lines = 0;
                using (var reader = File.OpenText(fileName))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine().Trim();
                        if (line == "")
                            continue;

                        var words = SplitString(line);
                        wordSet.UnionWith(words);

                        lines++;
                    }
                }

                Console.WriteLine($"Parsed {lines} lines");
            }

            var list = new List<string>(wordSet);
            list.Sort();

            File.WriteAllLines(dstFile, list);
            
            Console.WriteLine($"Total words {wordSet.Count}");
        }

        private static StringBuilder sb = new StringBuilder();

        public static IEnumerable<string> SplitString(string str)
        {
            sb.Clear();
            Char last = '\0';
            foreach (char c in str)
            {
                if ((c >= 'A' && c <= 'Z') 
                    || (c >= 'a' && c <= 'z')
                    || (sb.Length > 0 && ((c >= '0' && c <= '9') || (c == '\'') || (c == '-'))))
                {
                    sb.Append(Char.ToLower(c));
                    last = c;
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        var s = sb.ToString();
                        sb.Clear();

                        if (last == '\'' || last == '-')
                            s = s.Substring(0, s.Length-1); 
                        if (s.EndsWith("'s"))
                            s = s.Substring(0, s.Length-2); 
                            
                        yield return s; 
                    }
                }
            }

            yield return sb.ToString();
        }
    }
}