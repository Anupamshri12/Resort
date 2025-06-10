using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Resort.Application.Common.Interfaces;
using Resort.Application.Common.Utility;
using Resort.Domain.Entities;
using ResortAppication.Models;
using Syncfusion.Presentation;

namespace ResortAppication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _unitofwork;
        private readonly IWebHostEnvironment _webhostenvironment;
        public HomeController(IUnitofWork unitofwork ,IWebHostEnvironment webHostEnvironment)
        {
            _webhostenvironment = webHostEnvironment;
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            HomeVM homevm = new()
            {
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                Nights = 1,
                Villalist = _unitofwork.Villa.GetAll(includeproperties:"AmenityList")
            };
            return View(homevm);
        }
       
       
        [HttpPost]
       public IActionResult GetVillasByDate(int nights ,DateOnly CheckInDate)
        {
            var GetallVillas = _unitofwork.Villa.GetAll(includeproperties: "AmenityList");
            var villanumberlist = _unitofwork.VillaNumberInterface.GetAll().ToList();
            var booking = _unitofwork.BookingInterface.GetAll().ToList();
            foreach(var villa in GetallVillas)
            {
               int getnumbersvilla = SD.GetAvailaibleVillas(villa.Id , nights , CheckInDate.ToString() ,villanumberlist ,booking);
                if(getnumbersvilla == 0)
                {
                    villa.IsAvailable = false;
                }
                else
                {
                    villa.IsAvailable = true;
                }
            }
            HomeVM homevm = new()
            {
                Nights = nights,
                Villalist = GetallVillas,
                CheckInDate = CheckInDate,

            };
            return PartialView("_VillasList", homevm);
        }
        
        public IActionResult GeneratePPTExport(int id)
        {
            var getvilladetails = _unitofwork.Villa.Get(x => x.Id == id, includeproperties: "AmenityList");

            string basepath = _webhostenvironment.WebRootPath;
            string filepath = basepath + @"/exports/ExportVillaDetails.pptx";

            using IPresentation presentation = Presentation.Open(filepath);

            ISlide slide = presentation.Slides[0];

            IShape? shape = slide.Shapes.FirstOrDefault(x => x.ShapeName == "txtVillaName") as IShape;
            if(shape != null)
            {
                shape.TextBody.Text = getvilladetails.Name;
            }
            shape = slide.Shapes.FirstOrDefault(x => x.ShapeName == "txtVillaDescription") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = getvilladetails.Description;
            }
            shape = slide.Shapes.FirstOrDefault(x => x.ShapeName == "txtVillaSize") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = string.Format("Villa Size {0} sqft", getvilladetails.Sqft);
            }
            shape = slide.Shapes.FirstOrDefault(x => x.ShapeName == "txtOccupancy") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy {0} adults", getvilladetails.Occupancy) ;
            }
            shape = slide.Shapes.FirstOrDefault(x => x.ShapeName == "txtPricePerNight") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = string.Format("USD {0}/night", getvilladetails.Price.ToString("c"));
            }
            shape = slide.Shapes.FirstOrDefault(x => x.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if(shape != null)
            {
                shape.TextBody.Text = "AMENITY";
                var listitems = getvilladetails.AmenityList.Select(x => x.Name).ToList();
                foreach(var item in listitems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textpart = paragraph.AddTextPart(item);
                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textpart.Font.FontName = "system-ui";
                    textpart.Font.FontSize = 18;
                    textpart.Font.Color = ColorObject.FromArgb(144, 148, 152);

                }
            }
            shape = slide.Shapes.FirstOrDefault(x => x.ShapeName == "imgVilla") as IShape;
            basepath = _webhostenvironment.WebRootPath;
            if (shape != null)
            {
                byte[] imgdata;
                string imageurl;
                try
                {
                    imageurl = string.Format("{0}{1}", basepath, getvilladetails.ImageUrl);
                    imgdata = System.IO.File.ReadAllBytes(imageurl);

                }
                catch(Exception e)
                {
                    imageurl = string.Format("{0}{1}", basepath, "/images/placeholder.png");
                    imgdata = System.IO.File.ReadAllBytes(imageurl);
                }
                slide.Shapes.Remove(shape);
                using MemoryStream memorystream = new(imgdata);
                IPicture newpicture = slide.Pictures.AddPicture(memorystream, 60, 120, 300, 200);


            }
            MemoryStream stream = new();
            presentation.Save(stream);
            stream.Position = 0;

            return File(stream, "application/pptx", "villa.pptx");
        }
    }
}
