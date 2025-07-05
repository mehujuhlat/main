using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using client.Models;
using System.Diagnostics;

namespace client.Controllers
{
    public class EventsViewModel
    {
        public List<Event> ComingEvents { get; set; }
        public List<Event> PastEvents { get; set; }
    }

    public class EventsController : Controller
    {
        private readonly MehujuhlatContext _context;

        public EventsController(MehujuhlatContext context)
        {
            _context = context;
        }

        // GET: Event
        public async Task<IActionResult> Index()
        {
            var viewModel = new EventsViewModel
            {
                ComingEvents = await _context.Events.Where(p => p.Active == true).ToListAsync(),
                PastEvents = await _context.Events.Where(p=>p.Active==false).ToListAsync()
            };
            return View(viewModel);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.Include(p=>p.Images).Include(p => p.Tickets.Where(t => t.Valid==true)).FirstOrDefaultAsync(m => m.EventId == id );
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

   
        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
