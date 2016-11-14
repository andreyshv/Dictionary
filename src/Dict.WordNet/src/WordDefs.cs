namespace Dirct.WordNet
{
    public class Def
    {
        public string Value { get; set; }
        public string[] Synonims { get; set; }
    }

    public class Word
    {
        public string Value { get; set; }
        public string Pos { get; set; }
        public string[] Variants { get; set; }
        public Def[] Defs { get; set; }
        public string[] Morphs { get; set; }
    }
}