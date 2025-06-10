using Resort.Application.Common.Interfaces;
using Resort.Application.Common.Utility;
using Resort.Domain.Entities;
using Resort.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Infrastructure.Repository
{
    public class BookingRepository:Repository<Booking> ,IBookingInterface
    {
        private readonly ApplicationDbContext _dbContext;
      
        public BookingRepository(ApplicationDbContext dbContext ) : base(dbContext)
        {
            _dbContext = dbContext;
           
        }


        public void Update(Booking booking)
        {
            _dbContext.Update(booking);
        }

       public void UpdateStatus(int bookingid ,string bookingstatus ,int villanumber = 0)
        {
            var bookingval = _dbContext.Bookings.FirstOrDefault(u => u.Id == bookingid);
            if(bookingval != null)
            {
                bookingval.status = bookingstatus;
                if(bookingstatus == SD.StatusCheckedIn)
                {
                    bookingval.VillaNumber = villanumber;
                    bookingval.ActualCheckInDate = DateOnly.FromDateTime(DateTime.Now);
                }
                if(bookingstatus == SD.StatusCompleted)
                {
                    bookingval.ActualCheckOutDate = DateOnly.FromDateTime(DateTime.Now);
                }
            }
        }
        public void UpdateStripePaymentId(int bookingid ,string sessionid ,string paymentintentid)
        {
            var getbooking = _dbContext.Bookings.FirstOrDefault(u => u.Id == bookingid);
            if(getbooking != null)
            {
                if (!string.IsNullOrEmpty(sessionid))
                {
                    getbooking.StripeSessionId = sessionid;
                }
                if (!string.IsNullOrEmpty(paymentintentid))
                {
                    getbooking.StripePaymentIntentId = paymentintentid;
                    getbooking.PaymentDate = DateOnly.FromDateTime(DateTime.Now);
                    getbooking.IsPaymentSuccess = true;
                }
            }

        }
       
    }
}
