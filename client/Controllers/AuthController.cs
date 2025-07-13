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
    public class RecoverUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Id { get; set; }
    }

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
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

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

        public IActionResult Recover()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recover([Bind("Email")] RecoverUser recoverUser)
        {

            string recoverId = Helper.GenerateRandomString(10);
            RecoverPassword.Store(recoverId, recoverUser, TimeSpan.FromHours(10));

            string body = "";
            body += "Seuraavasta linkistä voit luoda uuden salasanan Mehujuhliin. <br>";
            body += "<a href=\"" + Helper.appUrl + "/Auth/NewPassword/" + recoverId + "\">" + Helper.appUrl + "/Auth/NewPassword/" + recoverId + "</a>";
            Helper.sendMail(recoverUser.Email, "Luo uusi salasana Mehujuhliin", body);
            return RedirectToAction("Login");
        }


        public IActionResult NewPassword(string id)
        {
            var recoverUser = RecoverPassword.get(id);
            if (recoverUser == null)
            {
                return Forbid();
            }
            recoverUser.Id = id;
            return View(recoverUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewPassword([Bind("Id,Password")] RecoverUser recoverUser)
        {
            var ru = RecoverPassword.get(recoverUser.Id);
            if (ru == null)
            {
                return Forbid();
            }

            var u = _context.Users.SingleOrDefault(x => x.Email == ru.Email);
            if ( u != null)
            {
                u.Password = Psw.HashPassword(recoverUser.Password, u.Salt);
                _context.Update(u);
                await _context.SaveChangesAsync();
                RecoverPassword.Remove(recoverUser.Id);
                TempData["pswChange"] = "Salasana on vaihdettu onnistuneesti.";
                return RedirectToAction("Login");
            }

            return View(nameof(Login));
        }

    }

}
