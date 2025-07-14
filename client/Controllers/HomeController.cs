using System;
using System.Diagnostics;
using client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        private readonly MehujuhlatContext _context;

        public HomeController(ILogger<HomeController> logger, MehujuhlatContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Ticket(string id)
        {
            /*
            var pTicket = await _context.Ptickets
                .Include(p => p.Ticket)
                .ThenInclude(t => t.Event)
                .FirstOrDefaultAsync(p => p.Code == id);
            if (pTicket == null)
            {
                return NotFound("Lippua ei löytynyt");
            }*/
            ViewData["Code"] = id;
            return View();
        }


        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Where(p=>p.Active==true).ToListAsync();
            return View(events);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
