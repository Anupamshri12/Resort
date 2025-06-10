using Microsoft.AspNetCore.Mvc;
using Resort.Infrastructure.Data;
using Resort.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using Resort.Application.Common.Interfaces;

using Resort.Application.Common.Utility;
using Microsoft.AspNetCore.Authorization;


namespace ResortAppication.Controllers
{
    //If role is admin then only this controller and UI will be displayed other wise access is denied;
    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IUnitofWork _unitofwork;
        public AmenityController(IUnitofWork unitofwork)
        {
            _unitofwork = unitofwork;

        }
        [HttpGet]
        public IActionResult Index()
        {


            var allVillas = _unitofwork.Amenity.GetAll(includeproperties:"Villa");
            return View(allVillas);
        }
        public IActionResult Create()
        {
            //Here IEnumerable is an interface which shows collection which can be iterated upon and selectlistitem to shows list where dropdown
            //is text and value is the submission to form post.
            IEnumerable<SelectListItem> villalist = _unitofwork.Villa.GetAll().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            //This Id is automatically converted from string to int due to model binding.
            //Here ViewData is dictionary which consist data list as value.
            //ViewData["Villalist"] = list;
            ViewData["Villalist"] = villalist;
            return View();
        }
        [HttpPost]
        public IActionResult Create(Amenity obj)
        {
            
            


            _unitofwork.Amenity.Add(obj);

            _unitofwork.Save();
            TempData["success"] = "Amenity created successfully";
            return RedirectToAction("Index", "Amenity");


        }

        public IActionResult Update(int amenityid)
        {
            IEnumerable<SelectListItem> villalist = _unitofwork.Villa.GetAll().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });
            ViewData["Villalist"] = villalist;

            var obj = _unitofwork.Amenity.Get(v => v.Id == amenityid);
            if (obj == null) return NotFound();
            return View(obj);
        }
        [HttpPost]
        public IActionResult Update(Amenity obj)
        {
            if (ModelState.IsValid && obj.VillaId > 0)
            {

                _unitofwork.Amenity.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Amenity updated successfully";
                return RedirectToAction("Index", "Amenity");
            }
          
            return View();
        }

        public IActionResult Delete(int amenityid)
        {
            var amenity = _unitofwork.Amenity.Get(v => v.Id == amenityid);
            if (amenity == null)
            {
                return NotFound();
            }
            _unitofwork.Amenity.Remove(amenity);
            _unitofwork.Save();
            TempData["success"] = "The amenity has been successfully deleted";
            return RedirectToAction("Index", "Amenity");
        }
    }
}
