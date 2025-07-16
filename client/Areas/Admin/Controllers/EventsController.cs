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
using System.Threading.Tasks;

namespace client.Areas.Admin.Controllers
{
    

    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")]
    public class EventsController : Controller
    {
        private readonly AzureBlobStorageService _blobStorage;
        private readonly MehujuhlatContext _context;

        public EventsController(AzureBlobStorageService blobStorage, MehujuhlatContext context)
        {
            _blobStorage = blobStorage;
            _context = context;
        }

        // GET: Admin/Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        // GET: Admin/Events/Details/5
        /*
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }*/

        // GET: Admin/Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,Name,Description,Active,Date,Program,DateEnd,MaxVisitors")] Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Admin/Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
     
            var @event = await _context.Events.Include(p=>p.Images).FirstOrDefaultAsync(e => e.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }
            
            return View(@event);
        }

        // POST: Admin/Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Name,Description,Active,Date,Program,DateEnd,MaxVisitors")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Events", new { area = "", id });
            }
            return View(@event);
        }

        // GET: Admin/Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Admin/Events/DeleteImage/5
        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Image = await _context.Images.FindAsync(id);
            if (Image == null)
            {
                return NotFound();
            }
            
            bool success  = await _blobStorage.DeleteImageAsync(Image.Url);
            if (success)
            {
                _context.Images.Remove(Image);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Edit), new { id = Image.EventId });
        }

        // POST: Admin/Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(int? EventId, IFormFile file, [Bind("Title,Description")] Image postImage)
        {
            try
            {
                if (!EventId.HasValue)
                {
                    return BadRequest("EventID puuttuu");
                }

                if (file == null || file.Length == 0 )
                {
                    return BadRequest("Tiedosto puuttuu");
                }
                var imageUrl = await _blobStorage.UploadImageAsync(file);
                    Image img = new Image
                    {
                        Description = postImage.Description,
                        Title = postImage.Title,
                        Url = imageUrl,
                        EventId = EventId.Value
                    };
                
                _context.Images.Add(img);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id= EventId.Value });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

    }
}
