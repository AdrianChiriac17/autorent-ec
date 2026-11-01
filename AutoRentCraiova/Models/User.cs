using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Models
{
    public enum UserRole
    {
        Customer,
        FleetManager,
        SystemAdministrator
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        public UserRole Role { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie să aibă exact 13 cifre.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "CNP-ul poate conține doar cifre.")]
        public string CNP { get; set; }
    }
}