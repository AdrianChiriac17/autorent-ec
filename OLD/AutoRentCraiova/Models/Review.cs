using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } 

        public string Comment { get; set; }

      
        public int UserId { get; set; }
        public User User { get; set; }

        public int CarId { get; set; }
        public Car Car { get; set; }
    }
}