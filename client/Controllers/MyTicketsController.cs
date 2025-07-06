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


    public class UserTicketsViewModel
    {
        public List<Pticket> UserTickets { get; set; }
        public List<Event> AllEvents { get; set; }
    }

    [Authorize]
    public class MyTicketsController : Controller
    {
        private readonly MehujuhlatContext _context;

        public MyTicketsController(MehujuhlatContext context)
        {
            _context = context;
        }

        // GET: MyTickets
        public async Task<IActionResult> Index()
        {
            int UserId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var viewModel = new UserTicketsViewModel
            {
                UserTickets = await _context.Ptickets.Where(p => p.UserId == UserId).Include(p => p.Ticket).ThenInclude(t => t.Event).Include(p => p.User).ToListAsync(),
                AllEvents = await _context.Events.Where(p=>p.Active==true).ToListAsync()
            };

            return View(viewModel);
        }

        // GET: MyTickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pticket = await _context.Ptickets
                .Include(p => p.Ticket)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PticketId == id);
            if (pticket == null)
            {
                return NotFound();
            }

            return View(pticket);
        }

        // GET: MyTickets/Buy
        public async Task<IActionResult> Buy(int? id)
        {
            Event e = await _context.Events.FindAsync(id);
            ViewData["EventId"] = id;
            ViewData["EventName"] = e.Name;

            ViewData["TicketId"] = new SelectList(_context.Tickets.Where(p => p.EventId == id &&  p.Valid == true)
                .Select(t => new {t.TicketId, DescriptionWithPrice = $"{t.Description} - {t.Price.ToString()}€" }),
                "TicketId",
                "DescriptionWithPrice");

            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: MyTickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,Firstname,Lastname,Address,Postalcode,City")] Pticket pticket)
        {
            // UserId, Price, Date, Code
            ModelState.Remove("UserId");
 
            ModelState.Remove("User");
            ModelState.Remove("Ticket");

            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Count > 0)
                    {
                        Debug.WriteLine($"Virhe kentässä '{entry.Key}':");
                        foreach (var error in entry.Value.Errors)
                        {
                            Debug.WriteLine($"- {error.ErrorMessage}");
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                Debug.WriteLine("ModelState.IsValid");
                int idx = -1;
                Int32.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out idx);
                pticket.UserId = idx;
                // Ticket t = await _context.Tickets.FindAsync(pticket.TicketId);
                Ticket t = await _context.Tickets.Include(t => t.Event).FirstOrDefaultAsync(t => t.TicketId == pticket.TicketId);
                pticket.Price = t.Price;
                pticket.Date = DateTime.Now;
                pticket.Code = Helper.GenerateRandomString(10);
                pticket.Valid = true;
                _context.Add(pticket);

                User u = await _context.Users.FindAsync(idx);
                string body = "";
                body += "Moi <strong>" + u.Nickname +"</strong><br>";
                body += "Olet ostanut lipun tapahtumaan " + t.Event.Name + " henkilölle " + pticket.Firstname + " " + pticket.Lastname+"<br>";
                body += "Tässä lipun qr<br>";
                body += "<img src=\""+Helper.appUrl+"/Api/GenQr/"+pticket.Code+"\"><br>";
                Helper.sendMail(u.Email, "Lipunosto mehujuhliin", body);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //ViewData["TicketId"] = new SelectList(_context.Tickets, "TicketId", "TicketId", pticket.TicketId);
            //ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", pticket.UserId);
            //return View(pticket);
            return RedirectToAction(nameof(Index));
        }

        // GET: MyTickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pticket = await _context.Ptickets.FindAsync(id);
            if (pticket == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "TicketId", "TicketId", pticket.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", pticket.UserId);
            return View(pticket);
        }

        // POST: MyTickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PticketId,UserId,TicketId,Price,Date,Firstname,Lastname,Address,Postalcode,City,Code")] Pticket pticket)
        {
            if (id != pticket.PticketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PticketExists(pticket.PticketId))
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
            ViewData["TicketId"] = new SelectList(_context.Tickets, "TicketId", "TicketId", pticket.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", pticket.UserId);
            return View(pticket);
        }

        // GET: MyTickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pticket = await _context.Ptickets
                .Include(p => p.Ticket)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PticketId == id);
            if (pticket == null)
            {
                return NotFound();
            }

            return View(pticket);
        }

        // POST: MyTickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pticket = await _context.Ptickets.FindAsync(id);
            if (pticket != null)
            {
                _context.Ptickets.Remove(pticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PticketExists(int id)
        {
            return _context.Ptickets.Any(e => e.PticketId == id);
        }
    }
}
