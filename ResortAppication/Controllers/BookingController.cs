using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Application.Common.Interfaces;
using Resort.Application.Common.Utility;
using Resort.Domain.Entities;
using Resort.Infrastructure.Repository;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using System.Linq;
using System.Security.Claims;
using Syncfusion.Drawing;
using System.Reflection.Metadata;
using Syncfusion.Pdf;
namespace ResortAppication.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitofWork _unitofWork;
        private readonly IWebHostEnvironment _webhostenvironment;
        public BookingController(IUnitofWork unitofWork , IWebHostEnvironment webHostEnvironment)
        {
            _webhostenvironment = webHostEnvironment;
            _unitofWork = unitofWork;
        }
        [Authorize]
        public IActionResult Index(string ?sortOrderName ,string ?sortOrderCost ,int ?pagesize , int ?pagenumber ,string status)
        {
            int currentpage = pagenumber ?? 1;
            int currentpagesize = pagesize ?? 5;
            ViewBag.SelectedPageSize = currentpagesize;
            ViewBag.Status = status;
            ViewBag.CurrentPage = currentpage;
            ViewData["CurrentSort1"] = sortOrderName;
            ViewData["CurrentSort2"] = sortOrderCost;

            ViewData["NameSortParam"] = sortOrderName == "name_asc" ? "name_desc" : "name_asc";
            ViewData["CostSortParam"] = sortOrderCost == "cost_asc" ? "cost_desc" : "cost_asc";
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var getallbookings = _unitofWork.BookingInterface.GetAll(u => u.UserId == userId ,includeproperties:"User");
            if (User.IsInRole(SD.Role_Admin))
            {
                getallbookings = _unitofWork.BookingInterface.GetAll();
            }
            if (!string.IsNullOrEmpty(status))
            {
                getallbookings = getallbookings.Where(b => b.status == status);
            }
            if(sortOrderName == "name_desc")
            {
                getallbookings = getallbookings.OrderByDescending(v => v.Name);
            }
            else if(sortOrderName == "name_asc")
            {
                getallbookings = getallbookings.OrderBy(v => v.Name);
            }
             if(sortOrderCost == "cost_asc")
            {
                getallbookings = getallbookings.OrderBy(v => v.TotalCost);
            }
            else if(sortOrderCost == "cost_desc")
            {
                getallbookings = getallbookings.OrderByDescending(v => v.TotalCost);
            }
            int totalbookings = getallbookings.Count();
            int totalpages = (int)Math.Ceiling(totalbookings / (double)currentpagesize);
            ViewBag.TotalPage = totalpages;
            var pagedBookings = getallbookings
                    .Skip((currentpage - 1) * currentpagesize)
                    .Take(currentpagesize);


            return View(pagedBookings);
            
        }
        [Authorize]
        public IActionResult FinalizeBooking(int villaId ,int nights ,string checkInDate)
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var userId = claimsidentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser user = _unitofWork.User.Get(u=>u.Id==  userId);
            if (!DateOnly.TryParse(checkInDate, out DateOnly parsedDate))
            {
                parsedDate = DateOnly.FromDateTime(DateTime.Now); // Fallback if parsing fails
            }
            Booking booking = new()
            {
                VillaId = villaId,
                Nights = nights,
                villa = _unitofWork.Villa.Get(v => v.Id == villaId, includeproperties: "AmenityList"),
                CheckInDate = parsedDate,
                UserId = userId,
              
                CheckOutDate = parsedDate.AddDays(nights),
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name
            };
            booking.TotalCost = booking.villa.Price * nights;
            return View(booking);

        }
        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var GetallVillas = _unitofWork.Villa.GetAll();
            var villanumberlist = _unitofWork.VillaNumberInterface.GetAll().ToList();
            var bookingall = _unitofWork.BookingInterface.GetAll().ToList();
           
                int getnumbersvilla = SD.GetAvailaibleVillas(booking.VillaId, booking.Nights, booking.CheckInDate.ToString(), villanumberlist, bookingall);
                if (getnumbersvilla == 0)
                {
                TempData["error"] = "Villa Sold out";
                 return RedirectToAction(nameof(FinalizeBooking), new
                 {
                     villaId = booking.VillaId,
                     nights = booking.Nights,
                     checkInDate = booking.CheckInDate
                 });
                }
                
            

            var getvilla = _unitofWork.Villa.Get(v => v.Id == booking.VillaId);
            booking.TotalCost = (double)(getvilla.Price * booking.Nights);
            booking.BookingDate = DateOnly.FromDateTime(DateTime.Now);
            booking.status = SD.StatusPending;
            _unitofWork.BookingInterface.Add(booking);
            _unitofWork.Save();



            var domain = Request.Scheme + "://" + Request.Host.Value + "/";

         
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };


            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = getvilla.Name
                    },
                },
                Quantity = 1,
            });
            var service = new SessionService();
            Session session = service.Create(options);
           

            _unitofWork.BookingInterface.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
            _unitofWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);



        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _unitofWork.BookingInterface.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitofWork.Save();
            TempData["success"] = "Successfully checked in";
            return RedirectToAction(nameof(BookingDetails), new { bookingid = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitofWork.BookingInterface.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitofWork.Save();
            TempData["success"] = "Checked Out";
            return RedirectToAction(nameof(BookingDetails), new { bookingid = booking.Id });
        }
        [HttpPost]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitofWork.BookingInterface.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitofWork.Save();
            TempData["success"] = "Booking successfully Cancelled";
            return RedirectToAction(nameof(BookingDetails), new { bookingid = booking.Id });
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingdetail = _unitofWork.BookingInterface.Get(u => u.Id == bookingId);
            if(bookingdetail.status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingdetail.StripeSessionId);
                if(session.PaymentStatus == "paid")
                {
                    _unitofWork.BookingInterface.UpdateStatus(bookingdetail.Id , SD.StatusApproved ,0);
                    _unitofWork.BookingInterface.UpdateStripePaymentId(bookingdetail.Id ,session.Id, session.PaymentIntentId);
                    _unitofWork.Save();

                }
            }
            return View(bookingId);
        }

        public IActionResult BookingDetails(int bookingid)
        {
            Booking bookingdetail = _unitofWork.BookingInterface.Get(b => b.Id == bookingid, includeproperties: "User,villa");
            int villaid = bookingdetail.VillaId;
            if (bookingdetail.VillaNumber == 0 && bookingdetail.status == SD.StatusApproved)
            {
                var getavailablevillas = GetVillaNumberAvailaible(bookingdetail);

                bookingdetail.villanumberlist = _unitofWork.VillaNumberInterface.GetAll(x => x.VillaId == villaid && getavailablevillas.Any(v => v == x.Villa_Number)).ToList();
            }
            return View(bookingdetail);

        }
        public IActionResult GenerateInvoice(int id ,string downloadType)
        {
            var bookingdata = _unitofWork.BookingInterface.Get(b => b.Id == id, includeproperties: "User,villa");

            string basepath = _webhostenvironment.WebRootPath;
            WordDocument wordDocument = new WordDocument();
            string datapath = basepath + @"/exports/BookingDetails.docx";
            using FileStream filestream = new(datapath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            wordDocument.Open(filestream, FormatType.Automatic);

            TextSelection textSelection = wordDocument.Find("xx_customer_name", false, true);
            WTextRange textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingdata.Name;

            textSelection = wordDocument.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingdata.Phone;

            textSelection = wordDocument.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingdata.Email;

            textSelection = wordDocument.Find("XX_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING_NUMBER- "+ bookingdata.Id.ToString();

            textSelection = wordDocument.Find("XX_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE- "+bookingdata.BookingDate.ToString();

            textSelection = wordDocument.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingdata.PaymentDate.ToString();

            textSelection = wordDocument.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingdata.CheckInDate.ToString();

            textSelection = wordDocument.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingdata.CheckOutDate.ToString();

            textSelection = wordDocument.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingdata.TotalCost.ToString("c");

            WTable table = new(wordDocument);
            table.TableFormat.Borders.LineWidth = 1f;
            table.TableFormat.Borders.Color = Color.Black;
            table.TableFormat.Paddings.Top = 7f;
            table.TableFormat.Paddings.Bottom = 7f;
            table.TableFormat.Borders.Horizontal.LineWidth = 1f;

            int rows = bookingdata.VillaNumber > 0 ? 3 : 2;
            table.ResetCells(rows, 4);
            WTableRow row0 = table.Rows[0];
            row0.Cells[0].AddParagraph().AppendText("NIGHTS");
            row0.Cells[0].Width = 80;
            row0.Cells[1].AddParagraph().AppendText("VILLA");
            row0.Cells[1].Width = 220;
            row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
            row0.Cells[3].AddParagraph().AppendText("TOTAL COST");
            row0.Cells[3].Width = 80;
            WTableRow row1 = table.Rows[1];
            row1.Cells[0].AddParagraph().AppendText(bookingdata.Nights.ToString());
            row1.Cells[0].Width = 80;
            row1.Cells[1].AddParagraph().AppendText(bookingdata.villa.Name);
            row1.Cells[1].Width = 220;
            row1.Cells[2].AddParagraph().AppendText((bookingdata.TotalCost/bookingdata.Nights).ToString());
            row1.Cells[3].AddParagraph().AppendText(bookingdata.TotalCost.ToString("c"));
            row1.Cells[3].Width = 80;
            if(rows > 2)
            {
                WTableRow row2 = table.Rows[2];
                row2.Cells[0].Width = 80;
                row2.Cells[1].AddParagraph().AppendText("Villa Number- "+bookingdata.VillaNumber.ToString());
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;



            }
            WTableStyle tableStyle = wordDocument.AddTableStyle("CustomStyle") as WTableStyle;
            tableStyle.TableProperties.RowStripe = 1;
            tableStyle.TableProperties.ColumnStripe = 2;
            tableStyle.TableProperties.Paddings.Top = 2;
            tableStyle.TableProperties.Paddings.Bottom = 1;
            tableStyle.TableProperties.Paddings.Left = 5.4f;
            tableStyle.TableProperties.Paddings.Right = 5.4f;

            ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
            firstRowStyle.CharacterFormat.Bold = true;
            firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
            firstRowStyle.CellProperties.BackColor = Color.Black;
            TextBodyPart bodytext = new TextBodyPart(wordDocument);
            table.ApplyStyle("CustomStyle");
            bodytext.BodyItems.Add(table);
            wordDocument.Replace("<ADDTABLEHERE>", bodytext, false, false);
          
            using DocIORenderer rendrere = new();
            MemoryStream stream = new();
            if(downloadType == "word")
            {
                wordDocument.Save(stream, FormatType.Docx);
                stream.Position = 0;

                return File(stream, "application/docx", "BookingDetails.docx");
            }
            else
            {
                PdfDocument pdfdocument = rendrere.ConvertToPDF(wordDocument);
                pdfdocument.Save(stream);
                stream.Position = 0;
                return File(stream, "application/pdf", "BookingDetails.pdf");
            }
           
        }

        public List<int> GetVillaNumberAvailaible(Booking bookingdetail)
        {
            List<int> availablevilla = new List<int>();
            var getvillanumber = _unitofWork.VillaNumberInterface.GetAll(v => v.VillaId == bookingdetail.VillaId  ).Select(x => x.Villa_Number).ToList();
            var bookedvillanumber = _unitofWork.BookingInterface.GetAll(b => b.VillaId == bookingdetail.VillaId
            && bookingdetail.CheckInDate > b.CheckInDate).Select(x=>x.VillaNumber).ToList();
            foreach(var villanumber in getvillanumber)
            {
                if (!bookedvillanumber.Contains(villanumber))
                {
                    availablevilla.Add(villanumber);
                }
            }
            return availablevilla;

        }
    }
}
