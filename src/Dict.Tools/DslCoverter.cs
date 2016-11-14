using System;
using System.Collections.Generic;
using System.IO;


namespace Dict.Tools
{
    public class DclConverter
    {
        public static void Convert(string fileName)
        {
            //TODO: check if file in ANSI encoding
            using (var stream = File.OpenRead(fileName))
            using (var reader = new StreamReader(stream, System.Text.Encoding.Unicode))
            {
                ReadHeader(reader);

                ReadCards(reader);
            }
        }

        private static void ReadHeader(StreamReader reader)
        {
            long pos;
            while (!reader.EndOfStream)
            {
                pos = reader.BaseStream.Position;
                var line = reader.ReadLine();
                if (line == "")
                    continue;

                if (!line.StartsWith("#"))
                {
                    reader.BaseStream.Position = pos;
                    break;
                }

                //TODO: header
            }
        }

        private static void ReadCards(StreamReader reader)
        {
            // http://lingvo.helpmax.net/ru/%D0%B2%D0%BE%D0%BF%D1%80%D0%BE%D1%81%D1%8B-%D0%B8-%D0%B7%D0%B0%D1%82%D1%80%D1%83%D0%B4%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F/dsl-compiler/%D1%81%D1%82%D1%80%D1%83%D0%BA%D1%82%D1%83%D1%80%D0%B0-%D1%81%D0%BB%D0%BE%D0%B2%D0%B0%D1%80%D1%8F-%D0%BD%D0%B0-%D1%8F%D0%B7%D1%8B%D0%BA%D0%B5-dsl/
            LingvoCard card = null;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == "")
                    continue;

                if (line[0] == ' ' || line[0] == '\t')
                {
                    // card body
                    if (card == null)
                        throw new InvalidOperationException("Card body without header");

                    card.Body.Add(line);
                }
                else
                {
                    // card header
                    // http://lingvo.helpmax.net/ru/%D0%B2%D0%BE%D0%BF%D1%80%D0%BE%D1%81%D1%8B-%D0%B8-%D0%B7%D0%B0%D1%82%D1%80%D1%83%D0%B4%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F/dsl-compiler/%D0%BE-%D1%81%D0%BE%D1%80%D1%82%D0%B8%D1%80%D0%BE%D0%B2%D0%BA%D0%B5-%D0%B7%D0%B0%D0%B3%D0%BE%D0%BB%D0%BE%D0%B2%D0%BA%D0%BE%D0%B2/
                    //TODO: escape symbols
                    if (card == null || card.Body.Count > 0)
                    {
                        card = new LingvoCard { Header = line, Body = new List<string>() };
                    }
                    else
                    {
                        card.Headers = new List<string>();
                        card.Headers.Add(line);
                    }
                }
            }
        }
    }

    public class LingvoCard
    {
        public string Header { get; set; }
        public List<string> Headers { get; set; }
        public List<string> Body { get; set; }
    }
}