using ARGI.DAL.Models;

namespace ARGI.DAL.Repository
{
    public interface IIrrigationRepository
    {
        Task<IEnumerable<IrrigationSchedule>> GetByDomeIdAsync(int domeId);
        Task<IEnumerable<IrrigationSchedule>> GetPendingSchedulesAsync();
        Task AddAsync(IrrigationSchedule schedule);
        void Delete(IrrigationSchedule schedule);
        Task<IrrigationSchedule> GetByIdAsync(int id);
        Task<bool> SaveChangesAsync();
    }
}
