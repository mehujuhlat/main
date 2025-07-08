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

    public class MessageIndexViewModel
    {
        public IEnumerable<Message> PrivateMessages { get; set; }
        public IEnumerable<Message> Messages { get; set; }
        public Message NewMessage { get; set; }  // Lomakkeen data
    }

    [Authorize]
    public class MyMessagesController : Controller
    {
        private readonly MehujuhlatContext _context;
        private readonly IMessageService _messageService;

        public MyMessagesController(MehujuhlatContext context, IMessageService messageService)
        {
            _context = context;
            _messageService = messageService;
        }

        // GET: Messages
        public async Task<IActionResult> Index()
        {

            int id = -1;
            Int32.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out id);
           

            var model = new MessageIndexViewModel
            {
             
            PrivateMessages = await _context.Messages
          .Include(m => m.Receiver)
          .Include(m => m.Sender)
          .Where(m => m.Private && (m.ReceiverId == id || m.SenderId == id))
          .OrderBy(m => m.Date)
          .ToListAsync(),

        
            Messages = await _context.Messages
          .Include(m => m.Receiver)
          .Include(m => m.Sender)
          .Where(m => !m.Private)
          .OrderBy(m => m.Date)
          .ToListAsync(),

            NewMessage = new Message
            {
                Private = TempData["KeepPrivateChecked"] != null ? (bool)TempData["KeepPrivateChecked"] : false
            }
            };
            var users = _context.Users.Where(u => u.UserId != id).ToList();
            var userList = new List<User>(users);
            userList.Insert(0, new User { UserId = -1, Nickname = "-- Lähetä kaikille --" });

            ViewData["ReceiverId"] = new SelectList(userList, "UserId", "Nickname", TempData["SelectedReceiverId"]);
            return View(model);
        }

        public async Task<IActionResult> Send([Bind("ReceiverId,Message1,Private")] Message message)
        {
            Debug.WriteLine("Message: "+message.Message1);
            Debug.WriteLine("ReceiverId: " + message.ReceiverId);
            Debug.WriteLine("Private: " + message.Private);

            message.NewMsg = true;
            int id = -1;
            Int32.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value,out id);

            if ( message.ReceiverId  == -1 )
                message.ReceiverId = null;

            message.SenderId = id;
            message.Date = DateTime.Now;
            _context.Add(message);

            await _context.SaveChangesAsync();

            TempData["KeepPrivateChecked"] = message.Private;
            TempData["SelectedReceiverId"] = message.ReceiverId;
            return RedirectToAction(nameof(Index));
        }


        // GET: Messages/Create
        public IActionResult Create()
        {
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "Nickname");
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "Nickname");
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MessageId,SenderId,ReceiverId,Message1,Date")] Message message)
        {
            ModelState.Remove("Sender");
            if (ModelState.IsValid)
            {
                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReceiverId"] = new SelectList(_context.Users, "UserId", "UserId", message.ReceiverId);
            ViewData["SenderId"] = new SelectList(_context.Users, "UserId", "UserId", message.SenderId);
            return View(message);
        }


        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }


            int  idx = -1;
            Int32.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out idx);


            bool admin = User.HasClaim(c => c.Type == "IsAdmin" && c.Value == "True");

            if (message.SenderId != idx && admin == false  )
            {
                return Forbid();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

          // TempData["KeepPrivateChecked"] = message.Private;
          //  TempData["SelectedReceiverId"] = message.ReceiverId;
            return RedirectToAction(nameof(Index));
        }


        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.MessageId == id);
        }
    }
}
