using client.App_Code;
using client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace client.Controllers
{
    [Authorize]
    public class MyDataController : Controller
    {
        private readonly MehujuhlatContext _context;

        public MyDataController(MehujuhlatContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int id = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.UserId != id )
            {
                return NotFound();
            }
            user.Password = "";
            return View(user);
        }

        public async Task<IActionResult> Remove(int id)
        {
            int userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (userId != id)
            {
                return Forbid();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Logout", "Auth");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("UserId,Firstname,Lastname,Email,Password,Nickname")] User user)
        {
            int id = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            ModelState.Remove("Salt");
            if (id != user.UserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                Debug.WriteLine("EDITOINTI VALIDI");

                var salt = Psw.GenerateSalt();
                user.Password = Psw.HashPassword(user.Password, salt);
                user.Salt = salt;

                if (User.HasClaim(c => c.Type == "IsAdmin" && c.Value == "True") && user.UserId == id)
                {
                    user.Admin = true;
                }

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            TempData["Alert"] = "Tapahtui virhe, tarkista syöttämäsi tiedot.";
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
