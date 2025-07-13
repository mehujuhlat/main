using client.App_Code;
using client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace client.Controllers
{
    public class RegisterController : Controller
    {
        private readonly MehujuhlatContext _context;

        public RegisterController(MehujuhlatContext context)
        {
            _context = context;
        }

        // GET: Register/Create
        public IActionResult Index()
        {
            return View();
        }

    
        public async Task<IActionResult> Confirm(string? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }
            User? u = NewUser.get(id);
            if (u == null)
                return NotFound();

            var salt = Psw.GenerateSalt();
            u.Password = Psw.HashPassword(u.Password, salt);
            u.Salt = salt;
            _context.Add(u);
            await _context.SaveChangesAsync();
            return View(u);
        }

        // POST: Register/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Firstname,Lastname,Email,Password,Nickname")] User user)
        {
            ModelState.Remove("Salt");

            if (ModelState.IsValid)
            {

                var emailInUse = await _context.Users.AnyAsync(k => k.Email == user.Email);
                if ( emailInUse)
                    ViewBag.emailInUse = "Sähköposti on jo käytössä";

                var emailIsCorrect = Helper.IsValidEmail(user.Email);
                if (!emailIsCorrect)
                    ViewBag.emailIsIncorrect = "Sähköposti on virheellinen";
                
                var nicknameInUse = await _context.Users.AnyAsync(k => k.Nickname == user.Nickname);
                if (nicknameInUse)
                    ViewBag.nicknameInUse = "Lempinimi on jo käytössä";

                if ( nicknameInUse || emailInUse || !emailIsCorrect)
                    return View("Index",user);

                string newId = Helper.GenerateRandomString(10);
                NewUser.Store(newId, user, TimeSpan.FromHours(10));

                //string body = "";
                //body += "Vahvista rekisteröitymisesi Mehujuhliin linkistä <br>";
                //body += "<a href=\""+Helper.appUrl+"/Register/Confirm/"+newId+"\">"+Helper.appUrl+"/Register/Confirm/"+newId+"</a>";
                //Helper.sendMail(user.Email, "Tervetuloa mehujuhliin "+user.Nickname, body);

                string body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: 'Arial', sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background-color: #f8d7da;
            padding: 20px;
            text-align: center;
            border-radius: 5px;
            margin-bottom: 20px;
        }}
        .button {{
            display: inline-block;
            padding: 10px 20px;
            background-color: #007bff;
            color: white !important;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
        }}
        .footer {{
            margin-top: 30px;
            font-size: 0.8em;
            color: #6c757d;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>Tervetuloa mehujuhliin, {user.Nickname ?? user.Firstname}!</h1>
    </div>
    
    <p>Hei {user.Firstname} {user.Lastname},</p>
    
    <p>Kiitos rekisteröitymisestäsi Mehujuhliimme! Vahvista vielä sähköpostiosoitteesi alla olevasta painikkeesta:</p>
    
    <div style='text-align: center;'>
        <a href='{Helper.appUrl}/Register/Confirm/{newId}' class='button'>Vahvista rekisteröityminen</a>
    </div>
    
    <p>Jos painike ei toimi, kopioi seuraava osoite selaimeesi:</p>
    <p><a href='{Helper.appUrl}/Register/Confirm/{newId}'>{Helper.appUrl}/Register/Confirm/{newId}</a></p>
    
    <div class='footer'>
        <p>Tämä on automaattinen viesti, älä vastaa tähän viestiin.</p>
        <p>© {DateTime.Now.Year} Mehujuhlat</p>
    </div>
</body>
</html>";

                Helper.sendMail(user.Email, $"Tervetuloa mehujuhliin {user.Nickname ?? user.Firstname}", body);




                ViewBag.newId = newId;
                return View("Validation", user);
            }
            return View(user);
        }
      
    }
}
