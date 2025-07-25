﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Mvc.Rendering;

namespace Resort.Domain.Entities
{
    public class RegisterVM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public  string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm Password")]
        public  string ConfirmPassword { get; set; }
       
        public string? RedirectUrl { get; set; }
        [Display(Name="Phone Number")]
        public string ? PhoneNumber { get; set; }
        public string? Role { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem>? RoleList { get; set; }
    }
}
