using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AutoRentCraiova.Models
{
    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie sa aiba exact 13 cifre.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "CNP-ul poate contine doar cifre.")]
        public string? CNP { get; set; }

        public IFormFile? DriverLicenseImage { get; set; }
        public IFormFile? IdCardImage { get; set; }

        public string? DriverLicenseUrl { get; set; }
        public string? IdCardUrl { get; set; }

        public DocumentStatus DocumentsStatus { get; set; }
    }
}
