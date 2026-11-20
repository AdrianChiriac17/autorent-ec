using System;
using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string BookingReference { get; set; } = string.Empty;

        public ReservationStatus Status { get; set; } = ReservationStatus.PendingApproval;

        public InsuranceType Insurance { get; set; } = InsuranceType.Basic;

        public bool HasGps { get; set; }
        public bool HasChildSeat { get; set; }
        public bool HasRoofRack { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        
        public int CarId { get; set; }
        public Car Car { get; set; }
    }
}