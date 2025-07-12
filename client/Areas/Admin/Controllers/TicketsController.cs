using client.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace client.Areas.Admin.Controllers
{

    public class TicketsEventsViewModel
    {
        public List<Ticket> Tickets { get; set; }
        public List<Event> AllEvents { get; set; }
    }

    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class TicketsController : Controller
    {
        private readonly MehujuhlatContext _context;

        public TicketsController(MehujuhlatContext context)
        {
            _context = context;
        }

        // GET: Admin/Tickets
        public async Task<IActionResult> Index(int id)
        {
            var viewModel = new TicketsEventsViewModel
            {
                Tickets = await _context.Tickets.Where(p => p.Event.EventId == id).Include(p => p.Event).ToListAsync(),
                AllEvents = await _context.Events.ToListAsync()
            };
            Event ev = await _context.Events.FindAsync(id);
            if (ev != null)
                ViewData["EventName"] = ev.Name;
;
            ViewData["Event"] = id;
            return View(viewModel);
        }

        // GET: Admin/Tickets/Details/5
        /*
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }*/

        // GET: Admin/Tickets/Create
        public async Task<IActionResult> CreateAsync(int id)
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventId");
            ViewData["Event"] = id;
            var ev = await _context.Events.FindAsync(id);
            if ( ev != null)
                ViewData["EventName"] = ev.Name;
            return View();
        }

        // POST: Admin/Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,EventId,Description,Type,Price,Day,Valid")] Ticket ticket)
        {
            ModelState.Remove("Event");
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
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventId", ticket.EventId);
            return View(ticket);
        }

        // GET: Admin/Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            var ev = await _context.Events.FindAsync(id);
            if (ev != null)
                ViewData["EventName"] = ev.Name;

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventId", ticket.EventId);
            ViewData["Event"] = ticket.EventId;
            return View(ticket);
        }

        // POST: Admin/Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,EventId,Description,Type,Price,Day,Valid")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }
            ModelState.Remove("Event");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
               
                return RedirectToAction(nameof(Index), new { id = ticket.EventId });
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventId", ticket.EventId);
            return View(ticket);
        }

        // GET: Admin/Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(m => m.TicketId == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Admin/Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.TicketId == id);
        }
    }
}
