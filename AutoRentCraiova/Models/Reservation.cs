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

       
        public int UserId { get; set; }
        public User User { get; set; }

        
        public int CarId { get; set; }
        public Car Car { get; set; }
    }
}