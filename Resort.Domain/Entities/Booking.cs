﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Domain.Entities
{
    public  class Booking
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [Required]
        public int VillaId { get; set; }
        [ForeignKey("VillaId")]
        public Villa villa { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string ? Phone { get; set; }

        [Required]
        public double TotalCost { get; set; }
        public int? Nights { get; set; }
        public string? status { get; set; }

        [Required]
        public DateOnly BookingDate { get; set; }
        [Required]
        public DateOnly CheckInDate { get; set; }
        [Required]
        public DateOnly CheckOutDate { get; set; }

        public bool IsPaymentSuccess { get; set; } = false;
        public DateOnly PaymentDate { get; set; }

        public string? StripeSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }

        public DateOnly ActualCheckInDate { get; set; }
        public DateOnly ActualCheckOutDate { get; set; }

        public int VillaNumber { get; set; }

        [NotMapped]
        public List<VillaNumber> villanumberlist { get; set; }

    }
}
