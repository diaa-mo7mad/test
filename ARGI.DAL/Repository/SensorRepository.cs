using ARGI.DAL.Data;
using ARGI.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Repository
{
    public class SensorRepository:ISensorRepository
    {

        private readonly ApplicationDbContext _context;

        public SensorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task AddReadingAsync(SensorReading reading)
        {
            await _context.SensorReadings.AddAsync(reading);
        }

     
        public async Task<IEnumerable<SensorReading>> GetDomeHistoryAsync(int domeId, int hours)
        {
            var cutoffTime = DateTime.Now.AddHours(-hours);

            return await _context.SensorReadings
                .Where(sr => sr.DomeId == domeId && sr.Timestamp >= cutoffTime)
                .OrderBy(sr => sr.Timestamp) 
                .ToListAsync();
        }

      
        public async Task<SensorReading> GetLatestReadingAsync(int domeId)
        {
            return await _context.SensorReadings
                .Where(sr => sr.DomeId == domeId)
                .OrderByDescending(sr => sr.Timestamp) 
                .FirstOrDefaultAsync();
        }

      
        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
