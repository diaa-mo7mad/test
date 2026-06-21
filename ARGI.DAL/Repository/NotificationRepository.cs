using ARGI.DAL.Data;
using ARGI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ARGI.DAL.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetByDomeIdAsync(int domeId)
        {
            return await _context.Notifications
                .Where(n => n.DomeId == domeId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public async Task<Notification> GetByIdAsync(int id)
        {
            return await _context.Notifications.FindAsync(id);
        }

        public void Update(Notification notification)
        {
            _context.Notifications.Update(notification);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
