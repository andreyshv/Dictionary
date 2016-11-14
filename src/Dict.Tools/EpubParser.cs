using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Pomelo.HtmlAgilityPack;

namespace Dict.Tools
{
    public class EpubParser
    {
        public static void ConvertEpubToTxt(string srcDir, string dstDir)
        {
            Directory.CreateDirectory(dstDir);
            HtmlDocument doc = new HtmlDocument();

            foreach (var fileName in Directory.GetFiles(srcDir, "*.epub"))
            {
                Console.WriteLine("Parse file: {0}", fileName);
                using (var stream = File.OpenRead(fileName))
                using (var arc = new ZipArchive(stream))
                using (var writer = new StringWriter())
                {
                    int cnt = 0;
                    var bookName = Path.Combine(dstDir, Path.GetFileNameWithoutExtension(fileName) + ".txt");
                    
                    // select all text files (chapters)
                    var enties = arc.Entries.Where(e => e.FullName.StartsWith("OEBPS/Text/"));
                    foreach (var e in enties)
                    {
                        using (var reader = new StreamReader(e.Open(), System.Text.Encoding.UTF8))
                        {
                            var html = reader.ReadToEnd();

                            // extract text
                            doc.LoadHtml(html);
                            ConvertTo(doc.DocumentNode, writer);
                            writer.WriteLine();

                            cnt++;
                        }
                    }

                    writer.Flush();
                    File.WriteAllText(bookName, writer.ToString());

                    Console.WriteLine("Converted {0} files of {1}", cnt, enties.Count());
                }
            }
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    // no title in epub
                    if ((parentName == "script") || (parentName == "style") || parentName == "title")
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    if (node.Name == "p")
                    {
                        // treat paragraphs as crlf
                        outText.Write("\r\n");
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }
    }
}