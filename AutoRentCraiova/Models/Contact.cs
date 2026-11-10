using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string? Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;
    }
}