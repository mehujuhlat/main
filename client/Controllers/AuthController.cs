using client.App_Code;
using client.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;



namespace client.Controllers
{

    public class LoginUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthController : Controller
    {
        private readonly MehujuhlatContext _context;

        public AuthController(MehujuhlatContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginUser loginUser)
        {

            if (ModelState.IsValid)
            {

                var LoggedUser = _context.Users.SingleOrDefault(x => x.Email == loginUser.Email);
                if (LoggedUser != null)
                {
                    var hash = Psw.HashPassword(loginUser.Password, LoggedUser.Salt);
                    if (hash == LoggedUser.Password)
                    {
                        Debug.WriteLine("KIRJAUTUMINEN ONNISTUI " + LoggedUser.Nickname);

                        bool Admin = false;
                        if (LoggedUser.Admin == true)
                        {
                            Debug.WriteLine("ADMININA SISÄÄN");
                            Admin = true;
                        }
                        
                        var claims = new List<Claim>
                         {
                           new Claim(ClaimTypes.GivenName, LoggedUser.Nickname),
                           new Claim(ClaimTypes.Name, LoggedUser.Email),
                           new Claim(ClaimTypes.NameIdentifier, LoggedUser.UserId.ToString()),
                           new Claim("IsAdmin", Admin.ToString())
                         };
                       
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity));
                        
                        return RedirectToAction("Index", "Home");
                    }
                    ViewBag.LoginMessage = "Väärä salasana";
                    return View();
                }
                else
                {
                    ViewBag.LoginMessage = "Väärä sähköpostiosoite"; ;
                    return View();
                }
            }
            return View(loginUser);
        }
    }
}
