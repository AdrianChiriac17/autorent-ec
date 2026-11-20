using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Models
{
    public enum UserRole
    {
        Customer,
        FleetManager,
        SystemAdministrator
    }

    public enum DocumentStatus
    {
        Missing,
        Pending,
        Approved,
        Rejected
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

        public string? Phone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        public UserRole Role { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie sa aiba exact 13 cifre.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "CNP-ul poate contine doar cifre.")]
        public string? CNP { get; set; }

        public string? DriverLicenseUrl { get; set; }

        public string? IdCardUrl { get; set; }

        public DocumentStatus DocumentsStatus { get; set; } = DocumentStatus.Missing;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}