using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Repetition
    {
        [Key]
        public int CardId { get; set; }
        public int Iteration { get; set; }
        public float Interval { get; set; }
        public float EasynessFactior { get; set; }
        public DateTime NextRepetition { get; set; }
    }
}
