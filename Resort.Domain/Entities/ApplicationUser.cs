using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Domain.Entities
{
    public class ApplicationUser:IdentityUser
    {
      
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}
