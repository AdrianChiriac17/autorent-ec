using System;
using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Models
{
    public class BookViewModel
    {
        [Required]
        public int CarId { get; set; }

        public Car? Car { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data Preluare")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data Returnare")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Asigurare")]
        public InsuranceType Insurance { get; set; } = InsuranceType.Basic;

        [Display(Name = "GPS (+15 RON/zi)")]
        public bool HasGps { get; set; }

        [Display(Name = "Scaun Copil (+25 RON/zi)")]
        public bool HasChildSeat { get; set; }

        [Display(Name = "Portbagaj Suplimentar (+35 RON/zi)")]
        public bool HasRoofRack { get; set; }
    }
}
