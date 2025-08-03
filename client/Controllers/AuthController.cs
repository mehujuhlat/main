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
        private readonly RecaptchaService _recaptchaService;

        public AuthController(MehujuhlatContext context, RecaptchaService recaptchaService)
        {
            _context = context;
            _recaptchaService = recaptchaService;
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
        public async Task<IActionResult> Login([Bind("Email,Password")] LoginUser loginUser, string gRecaptchaResponse)
        {
            Debug.WriteLine("gRecaptchaResponse " + gRecaptchaResponse);
            var recaptchaValid = await _recaptchaService.VerifyCaptcha(gRecaptchaResponse);
            if (!recaptchaValid)
            {
                ViewBag.recaptchaError = "ReCAPTCHA tunnistus epäonnistui.";
                return View();
            }

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
            var u = _context.Users.SingleOrDefault(x => x.Email == recoverUser.Email);

            if ( u == null )
            {
                ViewData["notFound"] = "Sähköpostia "+recoverUser.Email+" ei löydy.";
                return View();
            }
            string recoverId = Helper.GenerateRandomString(10);
            RecoverPassword.Store(recoverId, recoverUser, TimeSpan.FromHours(10));

            /*
            string body = "";
            body += "Seuraavasta linkistä voit luoda uuden salasanan Mehujuhliin. <br>";
            body += "<a href=\"" + Helper.appUrl + "/Auth/NewPassword/" + recoverId + "\">" + Helper.appUrl + "/Auth/NewPassword/" + recoverId + "</a>";
            Helper.sendMail(recoverUser.Email, "Luo uusi salasana Mehujuhliin", body);
            */

            // AI:n tekemä sähköposti 
            string body = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
    <div style='background-color: #6f42c1; color: white; padding: 20px; text-align: center;'>
        <h2>Mehujuhlat</h2>
    </div>
    
    <div style='padding: 20px;'>
        <p>Hei!</p>
        <p>Olet pyytänyt uuden salasanan luomista Mehujuhliin. Paina alla olevaa painiketta vaihtaaksesi salasanasi.</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{Helper.appUrl}/Auth/NewPassword/{recoverId}' 
               style='background-color: #6f42c1; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block; font-weight: bold;'>
                Luo uusi salasana
            </a>
        </div>
        
        <p>Jos et ole pyytänyt salasanan vaihtoa, voit jättää tämän viestin huomioimatta.</p>
        
        <p style='color: #666; font-size: 12px; margin-top: 30px;'>
            Tämä linkki on voimassa yhden tunnin. Jos linkki ei toimi, kopioi seuraava osoite selaimeen:<br>
            {Helper.appUrl}/Auth/NewPassword/{recoverId}
        </p>
    </div>
    
    <div style='background-color: #f8f9fa; color: #666; padding: 15px; text-align: center; font-size: 12px;'>
        © {DateTime.Now.Year} Mehujuhlat. Kaikki oikeudet pidätetään.
    </div>
</div>";
            Helper.sendMail(recoverUser.Email, "Luo uusi salasana Mehujuhliin", body);



            TempData["pswChange"] = "Linkki salasanan vaihtamiseen on lähetetty sähköpostiin "+recoverUser.Email;
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
            } else
            {
                TempData["pswChange"] = "Salasanan vaihto epäonnistui.";
            }

                return View(nameof(Login));
        }

    }

}
