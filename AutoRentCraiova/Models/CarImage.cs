using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Models
{
    public class CarImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [Required]
        public int CarId { get; set; }

        public virtual Car Car { get; set; }
    }
}