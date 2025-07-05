using client.Models;

namespace client.App_Code
{
    public interface IMessageService
    {
        int GetUnreadMessageCount(int userId);
        void MarkMessagesAsRead(int userId);
    }

    public class MessageService : IMessageService
    {
        private readonly MehujuhlatContext _context;

        public MessageService(MehujuhlatContext context)
        {
            _context = context;
        }

        public int GetUnreadMessageCount(int userId)
        {
            return _context.Messages
                .Count(m => m.ReceiverId == userId && m.NewMsg);
        }

        public void MarkMessagesAsRead(int userId)
        {
            var unreadMessages = _context.Messages
                .Where(m => m.ReceiverId == userId && m.NewMsg)
                .ToList();

            foreach (var msg in unreadMessages)
            {
                msg.NewMsg = false;
            }

            _context.SaveChanges();
        }
    }
}
