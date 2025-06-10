using Resort.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Application.Common.Interfaces
{
    public  interface IBookingInterface:IRepository<Booking>
    {
        void Update(Booking booking);
        void UpdateStatus(int bookingid, string bookingstatus ,int villanumber);
        void UpdateStripePaymentId(int bookingid, string sessionid, string paymentintentid);
    }
}
