using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Collection
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
