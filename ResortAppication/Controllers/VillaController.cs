using Microsoft.AspNetCore.Mvc;
using Resort.Infrastructure.Data;
using Resort.Domain.Entities;
using Resort.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
namespace ResortAppication.Controllers
{
    //No roles is defined so all roles are possible that can perform all actions
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IUnitofWork _unitofwork;
        private readonly IWebHostEnvironment _webhostenvironment;
        public VillaController(IUnitofWork unitofwork ,IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = unitofwork;
            _webhostenvironment = webHostEnvironment;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var allVillas = _unitofwork.Villa.GetAll();
            return View(allVillas);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if(obj.Name == obj.Description)
            {
                ModelState.AddModelError("Name", "The description cannot match Name exactly");
            }

            if (ModelState.IsValid)
            {
                if(obj.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_webhostenvironment.WebRootPath, @"Images\VillaImages");

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);

                    obj.ImageUrl = @"\Images\VillaImages\" + fileName;


                }
                else
                {
                    obj.ImageUrl = "https://placehold.co/600*400";
                }
                obj.Created_Time = DateTime.Now;
                _unitofwork.Villa.Add(obj);
                _unitofwork.Save();
                TempData["success"] = "Villa created successfully";
                return RedirectToAction("Index", "Villa");


            }
            return View();
        
        }

        public IActionResult Update(int VillaId)
        {
            var obj = _unitofwork.Villa.Get(v=>v.Id == VillaId);
            if (obj == null) return NotFound();
            return View(obj);
        }
        [HttpPost]
        public IActionResult Update(Villa obj)
        {
            if(ModelState.IsValid && obj.Id > 0)
            {
                if (obj.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_webhostenvironment.WebRootPath, @"Images\VillaImages");
                    if (!string.IsNullOrEmpty(obj.ImageUrl))
                    {
                       
                            var oldImagePath = Path.Combine(_webhostenvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        
                    }


                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);

                    obj.ImageUrl = @"\Images\VillaImages\" + fileName;


                }
                
                var createdTime = _unitofwork.Villa.GetCreatedTime(obj);
            

                obj.Created_Time = createdTime;

                obj.Updated_Time = DateTime.Now;
                _unitofwork.Villa.Update(obj);
                _unitofwork.Save();
                TempData["success"] = "Villa updated successfully";
                return RedirectToAction("Index", "Villa");
            }
            return View();
        }

        public IActionResult Delete(int VillaId)
        {
            var villa = _unitofwork.Villa.Get(v => v.Id == VillaId);
            if(villa == null)
            {
                return NotFound();
            }
            _unitofwork.Villa.Remove(villa);
            _unitofwork.Save();
            TempData["success"] = "The villa has been successfully deleted";
            return RedirectToAction("Index" ,"Villa");
        }

    }
}
