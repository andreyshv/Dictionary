using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Dict.WordNet.Model
{
    /// <summary>
    /// Lemma is lower case ASCII text of word or collocation.
    /// 
    /// </summary>
    public class Lemma
    {
        public int Id { get; set; }
        public string Value { get; set; }
        // Syntactic category: n for noun files, v for verb files, a for adjective files, r for adverb files.
        //public string Pos { get; set; }
        // Syntactic categories combined info string: n for noun files, v for verb files, a for adjective files, r for adverb files.
        public string Poses { get; set; }
        
        // sences of word
        public List<SynsetLemma> SynSets { get; set; }
        public List<Except> Excepts { get; set; }
        // diffent word writings 
        public List<Writing> Writings { get; set; }
    }

    /// <summary>
    /// Writing - ASCII form of a word as entered in the synset by the lexicographer.   
    /// The text of the word is case sensitive
    /// </summary>
    public class Writing
    {
        [Key]
        public string Value { get; set; }

        public int LemmaId { get; set; }
        public Lemma Lemma { get; set; }
    }

    /// <summary>
    /// SynSet (synonym set) is a set of words that are interchangeable in some context
    /// without changing the truth value of the preposition in which they are embedded.
    /// </summary>
    public class SynSet
    {
        public int Id { get; set; }
        // One character code indicating the synset type (n NOUN, v VERB, a ADJECTIVE, s ADJECTIVE SATELLITE, r ADVERB)
        public string Pos { get; set; }

        // sense synonims 
        public List<SynsetLemma> Lemmas { get; set; }
        public string Definition { get; set; }
        public string Example { get; set; }
    }

    /// <summary>
    /// SynsetLemma used to form many-to-many relationships between Lemma and SynSet entites
    /// Each synset may include one or more words and a every word may be included in different senses   
    /// </summary>
    public class SynsetLemma
    {
        public int LemmaId { get; set; }
        public Lemma Lemma { get; set; }

        public int SynSetId { get; set; }
        public SynSet SynSet { get; set; }
    }

    /// <summary>
    /// Exception
    /// </summary>
    public class Except
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public string MainForm { get; set; }

        public int? LemmaId { get; set; }
        public Lemma Lemma { get; set; }
    }

    public class WordNetContext : DbContext
    {
        public DbSet<Lemma> Lemmas { get; set; }
        public DbSet<Writing> Writings { get; set; }
        public DbSet<SynSet> SynSets { get; set; }
        public DbSet<Except> Excepts { get; set; }
        public DbSet<SynsetLemma> SynsetLemmas { get; set; }


        public WordNetContext(DbContextOptions options)
            : base(options)
        {
        }

        public static WordNetContext GetContext(string dbFileName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WordNetContext>();
            optionsBuilder.UseSqlite(string.Format("Filename={0}", dbFileName));

            return new WordNetContext(optionsBuilder.Options);
        }

        private Dictionary<string, PosGlosses[]> _wordDict;
        private List<string> _words;

        public Dictionary<string, PosGlosses[]> Defs
        {
            get
            {
                if (_wordDict == null)
                    InitDefs();

                return _wordDict;
            }
        }

        public List<string> Words
        {
            get
            {
                if (_words == null)
                    InitDefs();

                return _words;
            }
        }

        private void InitDefs()
        {
            _wordDict = Lemmas
                .Include(l => l.SynSets)
                   .ThenInclude(sl => sl.SynSet)
                .ToDictionary(
                    l => l.Value, 
                    l => l.SynSets
                        .GroupBy(sl => sl.SynSet.Pos, sl => new Gloss { Definition = sl.SynSet.Definition, Example = sl.SynSet.Example })
                        .Select(grp => new PosGlosses { Pos = grp.Key, Glosses = grp.ToArray() })
                        .ToArray()
                );

            _words = _wordDict.Keys.ToList();
            _words.Sort();
        }

        public bool IsDefinded(string word, PartOfSpeech pos)
        {
            // db query about 10 times slower than dictionary
            // return Lemmas.Any(l => l.Value == word && l.Poses.Contains(pos.symbol));
            
            // string posSym;
            // return Words.TryGetValue(word, out posSym) && posSym.Contains(pos.symbol);

            PosGlosses[] defs;
            return Defs.TryGetValue(word, out defs) && defs.Any(def => def.Pos == pos.symbol);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // low case lemma string
            modelBuilder.Entity<Lemma>()
                .HasIndex(l => l.Value);

            // modelBuilder.Entity<Lemma>()
            //     .HasIndex(l => l.Pos);

            modelBuilder.Entity<SynsetLemma>()
                .HasKey(sl => new { sl.LemmaId, sl.SynSetId });

            modelBuilder.Entity<SynsetLemma>()
                .HasOne(sl => sl.Lemma)
                .WithMany(l => l.SynSets)
                .HasForeignKey(sl => sl.LemmaId);

            modelBuilder.Entity<SynsetLemma>()
                .HasOne(sl => sl.SynSet)
                .WithMany(s => s.Lemmas)
                .HasForeignKey(sl => sl.SynSetId);
        }
    }

    public class Gloss
    {
        public string Pos { get; set; }
        public string Definition { get; set; }
        public string Example { get; set; }
    }

    public class PosGlosses
    {
        public string Pos { get; set; }
        public Gloss[] Glosses { get; set; }
    }
}
