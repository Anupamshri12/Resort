using Resort.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Application.Common.Utility
{
    public class BookingSummary
    {
        public DateOnly Date { get; set; }
        public int NewBookingCount { get; set; }
    }

    public class CustomerSummary
    {
        public DateOnly Date { get; set; }
        public int NewCustomerCount { get; set; }
    }

    public class MergedSummary
    {
        public DateOnly Date { get; set; }
        public int NewBookingCount { get; set; }
        public int NewCustomerCount { get; set; }
    }

    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static int GetAvailaibleVillas(int villaid ,int? nights ,string CheckInDate ,List<VillaNumber>villanumberlist ,
            List<Booking>booking)
        {
            List<int> bookingid = new List<int>();
           
            int totalvillasnumbers = villanumberlist.Where(v=>v.VillaId == villaid).Count();
            DateOnly checkindate = DateOnly.Parse(CheckInDate);
            int finalavail = int.MaxValue;
            for(int i = 0; i < nights; i++)
            {
                var isbookedvilla = booking.Where(b => b.CheckInDate <= checkindate.AddDays(i) &&
                b.CheckOutDate > checkindate.AddDays(i) && b.VillaId == villaid && b.status != SD.StatusCancelled);

                foreach(var booked in isbookedvilla)
                {
                    if (!bookingid.Contains(booked.Id)){
                        bookingid.Add(booked.Id );
                    }
                }
                
                int roomsavail = totalvillasnumbers - bookingid.Count;
                if (roomsavail == 0) return 0;
                else 
                {
                    if(finalavail > roomsavail)
                    finalavail = roomsavail;
                }
            }
            return finalavail;
        }
    }
}
