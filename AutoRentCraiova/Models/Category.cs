using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoRentCraiova.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu")]
        [Display(Name = "Nume Categorie")]
        public string Name { get; set; } = string.Empty; 

     
        public virtual ICollection<Car>? Cars { get; set; }
    }
}