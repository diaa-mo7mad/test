using ARGI.DAL.Data;
using ARGI.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ARGI.DAL.Repository
{
    public class IrrigationRepository : IIrrigationRepository
    {
        private readonly ApplicationDbContext _context;

        public IrrigationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IrrigationSchedule>> GetByDomeIdAsync(int domeId)
        {
            return await _context.IrrigationSchedules
                .Where(s => s.DomeId == domeId)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<IrrigationSchedule>> GetPendingSchedulesAsync()
        {
            var now = DateTime.Now;
            return await _context.IrrigationSchedules
                .Include(s => s.Dome)
                .Where(s => s.StartTime <= now && !s.IsExecuted)
                .ToListAsync();
        }

        public async Task AddAsync(IrrigationSchedule schedule)
        {
            await _context.IrrigationSchedules.AddAsync(schedule);
        }

        public void Delete(IrrigationSchedule schedule)
        {
            _context.IrrigationSchedules.Remove(schedule);
        }

        public async Task<IrrigationSchedule> GetByIdAsync(int id)
        {
            return await _context.IrrigationSchedules.FindAsync(id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
