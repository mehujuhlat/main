using client.Controllers;
using client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace client.Areas.Admin.Controllers
{

    public class PTicketsEventsViewModel
    {
        public List<Pticket> PTickets { get; set; }
        public List<Event> AllEvents { get; set; }
        public Event SelectedEvent { get; set; }
    }

    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class PticketsController : Controller
    {
        private readonly MehujuhlatContext _context;

        public PticketsController(MehujuhlatContext context)
        {
            _context = context;
        }

        // GET: Admin/Ptickets
        public async Task<IActionResult> Index(int id)
        {

            var viewModel = new PTicketsEventsViewModel
            {
                PTickets = await _context.Ptickets.Where(p => p.Ticket.Event.EventId == id).Include(p => p.Ticket).ThenInclude(t => t.Event).Include(p => p.User).ToListAsync(),
                AllEvents = await _context.Events.ToListAsync(),
                SelectedEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id)
            };
            Event ev = await _context.Events.FindAsync(id);
            if (ev != null)
                ViewData["EventName"] = ev.Name;

            decimal? sum = 0;
            int ticketCount = 0;
            foreach ( Pticket p in viewModel.PTickets)
            {
                if ( p.Price.HasValue )
                sum += p.Price.Value;
                ticketCount++;
            }
            if (sum != null)
                ViewData["TotalSum"] = sum;
            ViewData["id"] = id;
            ViewData["TotalTickets"] = ticketCount;

            return View(viewModel);
        }

        // GET: Admin/Ptickets/Details/5
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

        // GET: Admin/Ptickets/Create
        public IActionResult Create()
        {
            ViewData["TicketId"] = new SelectList(_context.Tickets, "TicketId", "TicketId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Admin/Ptickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PticketId,UserId,TicketId,Price,Date,Firstname,Lastname,Address,Postalcode,City,Code,Valid")] Pticket pticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "TicketId", "TicketId", pticket.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", pticket.UserId);
            return View(pticket);
        }

        // GET: Admin/Ptickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var pticket = await _context.Ptickets.FindAsync(id);
            var pticket = await _context.Ptickets
                                .Include(p => p.Ticket)
                                .ThenInclude(t => t.Event)
                                .FirstOrDefaultAsync(p => p.PticketId == id); 

            if (pticket == null)
            {
                return NotFound();
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets.Where(p=>p.EventId==pticket.Ticket.EventId), "TicketId", "Description", pticket.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Nickname", pticket.UserId);
            return View(pticket);
        }

        // POST: Admin/Ptickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PticketId,UserId,TicketId,Price,Date,Firstname,Lastname,Address,Postalcode,City,Code,Valid,Cancel")] Pticket pticket)
        {

            if (id != pticket.PticketId)
            {
                return NotFound();
            }

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
                var tick = await _context.Ptickets.Include(t => t.Ticket).FirstOrDefaultAsync(m => m.TicketId == pticket.TicketId);
                return RedirectToAction(nameof(Index), new { id = tick.Ticket.EventId });
            }
            ViewData["TicketId"] = new SelectList(_context.Tickets, "TicketId", "TicketId", pticket.TicketId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", pticket.UserId);
            return View(pticket);
        }

        // GET: Admin/Ptickets/Delete/5
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

        // POST: Admin/Ptickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pticket = await _context.Ptickets
                .Include(p => p.Ticket)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PticketId == id);

            if (pticket != null)
            {
                _context.Ptickets.Remove(pticket);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { id = pticket.Ticket.EventId });
            // return RedirectToAction(nameof(Index));
        }

        private bool PticketExists(int id)
        {
            return _context.Ptickets.Any(e => e.PticketId == id);
        }
    }
}
