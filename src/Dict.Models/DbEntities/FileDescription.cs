using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class WordToFile
    {
        public int Id { get; set; }
        public string Word { get; set; }
        public string FileName { get; set; }
    }

    public class FileDescription
    {
        [Key]
        public string Name { get; set; }
        public FileType FileType { get; set; }
        public string Hash { get; set; }
    }

    public enum FileType
    {
        None,
        Image,
        Sound
    }
}
