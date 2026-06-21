using ARGI.DAL.Models;

namespace ARGI.DAL.Repository
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByDomeIdAsync(int domeId);
        Task AddAsync(Notification notification);
        Task<Notification> GetByIdAsync(int id);
        void Update(Notification notification);
        Task<bool> SaveChangesAsync();
    }
}
