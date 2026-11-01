using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AutoRentCraiova.Models
{
    public enum CarStatus
    {
        Disponibila = 0,
        InMentenanta = 1,
        Inchiriata = 2
    }

    public class Car
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Brandul este obligatoriu")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Modelul este obligatoriu")]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        public string? Transmission { get; set; }

        public string? FuelType { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        public decimal PricePerDay { get; set; }

        public bool HasAC { get; set; }
        
        [Range(2, 9, ErrorMessage = "Numarul de locuri trebuie sa fie intre 2 si 9")]
        public int Seats { get; set; }
        
        [Range(2, 5, ErrorMessage = "Numarul de usi trebuie sa fie intre 2 si 5")]
        public int Doors { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        public decimal AverageConsumption { get; set; }
        
        [Required]
        public int CategoryId { get; set; }

        public string? ImageUrl { get; set; }
        
        [Required(ErrorMessage = "Seria de sasiu (VIN) este obligatorie")]
        public string VIN { get; set; }

        [Required(ErrorMessage = "Numarul de inmatriculare este obligatoriu")]
        public string LicensePlate { get; set; }

        [NotMapped]
        public List<IFormFile>? ImageFiles { get; set; }

        public CarStatus Status { get; set; } = CarStatus.Disponibila;
      
        public virtual Category? Category { get; set; }
        
        public virtual ICollection<CarImage> Images { get; set; } = new List<CarImage>();
    }
}