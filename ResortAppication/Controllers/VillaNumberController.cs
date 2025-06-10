using Microsoft.AspNetCore.Mvc;
using Resort.Infrastructure.Data;
using Resort.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Data;
using Dapper;
using Resort.Application.Common.Interfaces;
namespace ResortAppication.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IUnitofWork _unitofwork;
        public VillaNumberController(IUnitofWork unitofwork)
        {
            _unitofwork = unitofwork;
            
        }
        [HttpGet]
        public IActionResult Index()
        {

            
            var allVillas = _unitofwork.VillaNumberInterface.GetAll(includeproperties: "Villa");
            return View(allVillas);
        }
        public IActionResult Create()
        {
            //Here IEnumerable is an interface which shows collection which can be iterated upon and selectlistitem to shows list where dropdown
            //is text and value is the submission to form post.
            IEnumerable<SelectListItem> villalist =_unitofwork.Villa.GetAll().Select(v => new SelectListItem
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
        public IActionResult Create(VillaNumber obj)
        {
            var villanumber = _unitofwork.VillaNumberInterface.Get(v=>v.Villa_Number == obj.Villa_Number);
            if(villanumber != null)
            {
                TempData["error"] = "Villa number already exist";
                return View();


            }


            _unitofwork.VillaNumberInterface.Add(obj);

            _unitofwork.Save();
                TempData["success"] = "Villa Number created successfully";
                return RedirectToAction("Index", "VillaNumber");
            
            
        }

        public IActionResult Update(int villanumberid)
        {
            IEnumerable<SelectListItem> villalist = _unitofwork.Villa.GetAll().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });
            ViewData["Villalist"] = villalist;

            var obj = _unitofwork.VillaNumberInterface.Get(v=>v.Villa_Number == villanumberid);
            if (obj == null) return NotFound();
            return View(obj);
        }
        [HttpPost]
        public IActionResult Update(VillaNumber obj)
        {
            if (ModelState.IsValid && obj.VillaId > 0)
            {

                _unitofwork.VillaNumberInterface.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Villa Number updated successfully";
                return RedirectToAction("Index", "VillaNumber");
            }
            return View();
        }

        public IActionResult Delete(int villanumberid)
        {
            var villanumber = _unitofwork.VillaNumberInterface.Get(v => v.Villa_Number == villanumberid);
            if (villanumber == null)
            {
                return NotFound();
            }
            _unitofwork.VillaNumberInterface.Remove(villanumber);
            _unitofwork.Save();
            TempData["success"] = "The villa number has been successfully deleted";
            return RedirectToAction("Index", "VillaNumber");
        }

    }
}
