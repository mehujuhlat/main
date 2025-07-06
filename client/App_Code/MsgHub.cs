using client.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;


namespace client.App_Code
{
    public class MsgHub : Hub
    {
        private readonly MehujuhlatContext _context;

        public MsgHub(MehujuhlatContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string receiverStr, string message, bool Private)
        {
            var idstr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int id = -1;
            int.TryParse(idstr, out id);
            int.TryParse(receiverStr, out int receiverId);

            var msg = new Message();

            msg.SenderId = id;

            if (receiverId >= 0)
                msg.ReceiverId = receiverId;
            else
                msg.ReceiverId = null;

            msg.Message1 = message;
            msg.Private = Private;
            msg.NewMsg = true;
            msg.Date = DateTime.Now;
            
            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            var receiverNickname = "";
            var sender = await _context.Users.FirstOrDefaultAsync(m => m.UserId == id);
            var receiver = await _context.Users.FirstOrDefaultAsync(m => m.UserId == receiverId);

            if (receiver?.Nickname != null)
                receiverNickname = receiver.Nickname;
                
            if (!Private)
                await Clients.Others.SendAsync("ReceiveMessage", id, sender?.Nickname, receiverId, receiverNickname, msg.MessageId, msg.Message1, msg.Date?.ToString(), false, false);
            else
                await Clients.User(receiverStr).SendAsync("ReceiveMessage", id, sender?.Nickname, receiverId, receiver?.Nickname, msg.MessageId, msg.Message1, msg.Date?.ToString(),false,true);
            
            await Clients.User(idstr).SendAsync("ReceiveMessage", id, sender?.Nickname, receiverId, receiverNickname, msg.MessageId, msg.Message1, msg.Date?.ToString(), true, Private);

        }

    }
}
