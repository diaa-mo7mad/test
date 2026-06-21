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

        // 1. إضافة القراءة
        public async Task AddReadingAsync(SensorReading reading)
        {
            await _context.SensorReadings.AddAsync(reading);
        }

        // 2. جلب تاريخ القراءات (مثلاً آخر 24 ساعة) وترتيبها من الأقدم للأحدث ليقرأها التشارت بشكل صحيح
        public async Task<IEnumerable<SensorReading>> GetDomeHistoryAsync(int domeId, int hours)
        {
            var cutoffTime = DateTime.Now.AddHours(-hours);

            return await _context.SensorReadings
                .Where(sr => sr.DomeId == domeId && sr.Timestamp >= cutoffTime)
                .OrderBy(sr => sr.Timestamp) // الترتيب تصاعدي حسب الوقت لزوم الـ Chart
                .ToListAsync();
        }

        // 3. جلب أحدث قراءة وصلت للمزرعة
        public async Task<SensorReading> GetLatestReadingAsync(int domeId)
        {
            return await _context.SensorReadings
                .Where(sr => sr.DomeId == domeId)
                .OrderByDescending(sr => sr.Timestamp) // الترتيب تنازلي لنأخذ أول واحدة
                .FirstOrDefaultAsync();
        }

        // 4. الحفظ
        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}
