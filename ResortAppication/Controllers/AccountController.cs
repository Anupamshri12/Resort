using Microsoft.AspNetCore.Identity;

using Resort.Application.Common.Interfaces;
using Resort.Domain.Entities;


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Resort.Application.Common.Utility;
using Microsoft.Win32;


namespace ResortAppication.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitofWork _unitofwork;
        //Helpers method

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
       

        public AccountController(IUnitofWork unitofWork, UserManager<ApplicationUser>userManager ,SignInManager<ApplicationUser>signInManager ,
            RoleManager<IdentityRole> roleManager)
        {
            _unitofwork = unitofWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public IActionResult Register(string returnurl = null)
        {
            returnurl ??= Url.Content("~/");

            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();

            }

            RegisterVM register = new()
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                RedirectUrl = returnurl
            };

            return View(register);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM register)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new()
                {
                    Name = register.Name,
                    Email = register.Email,
                    PhoneNumber = register.PhoneNumber,
                    NormalizedEmail = register.Email.ToUpper(),
                    CreatedAt = DateTime.Now,
                    UserName = register.Email,
                    EmailConfirmed = true

                };

                var result = await _userManager.CreateAsync(user, register.Password);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(register.Role))
                    {
                        await _userManager.AddToRoleAsync(user, register.Role);
                    }
                    else
                    {
                        //else assigning low level role as customer;
                        await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    }
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    if (!string.IsNullOrEmpty(register.RedirectUrl))
                    {
                        return LocalRedirect(register.RedirectUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
    ;
                }

                foreach (var errors in result.Errors)
                {
                    //combine all error and display as once;
                    ModelState.AddModelError("", errors.Description);
                }
            }
            register.RoleList = _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            });
            return View(register);


        }
        public IActionResult Login(string returnurl = null)
        {
            returnurl ??= Url.Content("~/");
            LoginVM login = new()
            {
                RedirectUrl = returnurl
            };
            return View(login);
        }
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM login)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, login.RememberMe ,lockoutOnFailure:false);
                if (result.Succeeded)
                {
                    TempData["success"] = "Successfully logged in";
                    if (User.IsInRole(SD.Role_Admin))
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(login.RedirectUrl))
                        {
                            return LocalRedirect(login.RedirectUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
    ;
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                }
            }
            return View(login);


        }
        public IActionResult Forgot()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Forgot(ForgotVM forgotvm)
        {
            if (ModelState.IsValid)
            {
                var getuser = _unitofwork.User.Get(u => u.Email == forgotvm.Email);
                var token = await _userManager.GeneratePasswordResetTokenAsync(getuser);

                var result = await _userManager.ResetPasswordAsync(getuser, token ,forgotvm.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid password change attempt");

                }

            }
            return View(forgotvm);
        }
        public IActionResult AccessDenied()
        {

            return View();
        }
    }
}
