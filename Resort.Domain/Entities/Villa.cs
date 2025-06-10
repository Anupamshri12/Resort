using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Domain.Entities
{
   public class Villa
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }
        public string ? Description { get; set; }
        [Display(Name = "Price per night")]
        [Range(500 ,50000)]
        public double Price { get; set; }
        public int Sqft { get; set; }
        [Range(1 ,100)]
        public int Occupancy { get; set; }
        [Display(Name = "Image Url")]
        [NotMapped]
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? Created_Time { get; set; }
        public DateTime? Updated_Time { get; set; }
        [ValidateNever]
        public IEnumerable<Amenity>? AmenityList { get; set; }
        [NotMapped]
        public bool IsAvailable { get; set; } = true;

    }
}
