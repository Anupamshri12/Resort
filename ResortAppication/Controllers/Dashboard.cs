using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Application.Common.Interfaces;
using Resort.Application.Common.Utility;
using Resort.Domain.Entities;
using System.Linq;

namespace ResortAppication.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    
    public class Dashboard : Controller
    {
        static int month = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        private readonly IUnitofWork _unitofWork;
        public readonly DateOnly prevDateTime = new(DateTime.Now.Year, month, 1);
        public readonly DateTime prevDateTime1 = new(DateTime.Now.Year, month, 1);

        public readonly DateOnly currDateTime = new (DateTime.Now.Year ,DateTime.Now.Month,1);
        public readonly DateTime currDateTime1 = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public Dashboard(IUnitofWork unitofWork)
        {
            _unitofWork = unitofWork;
        }
        public IActionResult Index()
        {
            List<RadialChartVM> radiallist = new List<RadialChartVM>();
            var getallbookings = _unitofWork.BookingInterface.GetAll(v => v.status != SD.StatusPending ||
            v.status != SD.StatusCancelled);
            
            var countbycurrmonth = getallbookings.Count(b => b.BookingDate >= currDateTime && b.BookingDate <=
            DateOnly.FromDateTime(DateTime.Now));

            var countbyprevmonth = getallbookings.Count(b => b.BookingDate >= prevDateTime && b.BookingDate <
            currDateTime);

            RadialChartVM bookingmodel = new RadialChartVM();
            int ratiocalculated = 100;
            if(countbyprevmonth != 0)
            {
                ratiocalculated = Convert.ToInt32((double)(countbycurrmonth-countbyprevmonth)/countbyprevmonth*100);
            }
            bookingmodel.TotalCount = getallbookings.Count();
            bookingmodel.IncreaseDecreaseAmount = countbycurrmonth-countbyprevmonth;
            bookingmodel.HasRationIncreased = countbycurrmonth > countbyprevmonth;
            bookingmodel.series =ratiocalculated ;
            RadialChartVM usermodel = new RadialChartVM();
            
            var getallusers = _unitofWork.User.GetAll();
            var countbycurruser = getallusers.Count(x => x.CreatedAt.Date >= currDateTime1.Date && x.CreatedAt.Date <=
            DateTime.Now.Date);
            var prevcountbyuser = getallusers.Count(x => x.CreatedAt.Date >= prevDateTime1.Date && x.CreatedAt.Date <
            currDateTime1.Date);
            int ratiocalculated1 = 100;
            if (prevcountbyuser != 0)
            {
                ratiocalculated1 = Convert.ToInt32((double)(countbycurruser - prevcountbyuser) / prevcountbyuser * 100);
            }
            usermodel.TotalCount = getallusers.Count();
            usermodel.IncreaseDecreaseAmount = countbycurruser-prevcountbyuser;
            usermodel.HasRationIncreased = countbycurruser > prevcountbyuser;
            usermodel.series =  ratiocalculated1 ;

            RadialChartVM revenuemodel = new RadialChartVM();
            var getrevenuebookings = _unitofWork.BookingInterface.GetAll(x=>x.status == SD.StatusCompleted);
            var getcurrrevenue = getrevenuebookings.Where(b => b.BookingDate >= currDateTime && b.BookingDate <=
            DateOnly.FromDateTime(DateTime.Now)).Sum(b=>b.TotalCost);
            var getprevrevenue = getrevenuebookings.Where(b => b.BookingDate >= prevDateTime && b.BookingDate <
            currDateTime).Sum(b => b.TotalCost);
            int ratiocalculated2 = 100;
            if (getprevrevenue != 0)
            {
                ratiocalculated2 = Convert.ToInt32((double)(getcurrrevenue - getprevrevenue) / getprevrevenue * 100);
            }
            revenuemodel.TotalCount = (decimal)getrevenuebookings.Sum(x=>x.TotalCost);
            revenuemodel.IncreaseDecreaseAmount = (decimal)(getcurrrevenue - getprevrevenue);
            revenuemodel.HasRationIncreased = getcurrrevenue > getprevrevenue;
            revenuemodel.series = ratiocalculated2;

            radiallist.Add(bookingmodel);
            radiallist.Add(usermodel);
            radiallist.Add(revenuemodel);

            var getall_bookings = _unitofWork.BookingInterface.GetAll(x => x.BookingDate >= DateOnly.FromDateTime(DateTime.Now).AddDays(-30)
            && x.status != SD.StatusPending || x.status != SD.StatusCancelled);

            var newcustomers = getallbookings.GroupBy(x => x.UserId).Where(x => x.Count() == 1);
            int newbookings = newcustomers.Count();
            int returnedbookings = getallbookings.Count() - newbookings;
            RadialChartVM newpiechart = new RadialChartVM();
            newpiechart.Serial = new int[] { newbookings, returnedbookings };
            newpiechart.Labels = new string[] { "New Customer Bookings", "Returned Customers" };
            radiallist.Add(newpiechart);

            //NEW LINE CHART ANALYSIS
            // Assume these are filled correctly from _unitofWork.GetAll() and converted to lists
            List<BookingSummary> bookingdata = _unitofWork.BookingInterface.GetAll(
                b => b.BookingDate >= DateOnly.FromDateTime(DateTime.Now.AddDays(-30)) &&
                     b.BookingDate <= DateOnly.FromDateTime(DateTime.Now))
                .GroupBy(x => x.BookingDate)
                .Select(g => new BookingSummary
                {
                    Date = g.Key,
                    NewBookingCount = g.Count()
                })
                .ToList();

            List<CustomerSummary> customerdata = _unitofWork.User.GetAll(
                b => b.CreatedAt.Date >= DateTime.Now.AddDays(-30).Date &&
                     b.CreatedAt.Date <= DateTime.Now.Date)
                .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt))
                .Select(g => new CustomerSummary
                {
                    Date = g.Key,
                    NewCustomerCount = g.Count()
                })
                .ToList();

            // ✅ GroupJoin with full type arguments
            var leftjoin = Enumerable.GroupJoin<BookingSummary, CustomerSummary, DateOnly, MergedSummary>(
                bookingdata,
                customerdata,
                booking => booking.Date,
                customer => customer.Date,
                (booking, customerGroup) => new MergedSummary
                {
                    Date = booking.Date,
                    NewBookingCount = booking.NewBookingCount,
                    NewCustomerCount = customerGroup.Select(x => x.NewCustomerCount).FirstOrDefault()
                }).ToList();

            var rightjoin = Enumerable.GroupJoin<CustomerSummary, BookingSummary, DateOnly, MergedSummary>(
                customerdata,
                bookingdata,
                customer => customer.Date,
                booking => booking.Date,
                
                (customerGroup, booking) => new MergedSummary
                {
                    Date = customerGroup.Date,
                    NewBookingCount = booking.Select(x => x.NewBookingCount).FirstOrDefault(),

                    NewCustomerCount = customerGroup.NewCustomerCount
                }).ToList();

            var mergeddata = leftjoin.Union(rightjoin).OrderBy(x => x.Date).ToList();

            var categories = mergeddata.Select(x => x.Date.ToString("MM/dd/yyyy")).ToArray();
            var newbookingcount = mergeddata.Select(x => x.NewBookingCount).ToArray();
            var newcustomercount = mergeddata.Select(x => x.NewCustomerCount).ToArray();

            List<LineChart> chartlist = new()
            {
                new LineChart
                {
                    Name = "New Bookings",
                    data = newbookingcount
                },
                new LineChart
                {
                    Name = "New Customers",
                    data = newcustomercount
                }
            };

            RadialChartVM linechartvm = new RadialChartVM();
            linechartvm.Collections = categories;
            linechartvm.seriesal = chartlist;
            radiallist.Add(linechartvm);
            return View(radiallist);













        }
    }
}
