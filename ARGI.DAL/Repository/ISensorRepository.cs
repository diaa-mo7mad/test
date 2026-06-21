using ARGI.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Repository
{
    public interface ISensorRepository
    {
        Task AddReadingAsync(SensorReading reading);
        Task<IEnumerable<SensorReading>> GetDomeHistoryAsync(int domeId, int hours);
        Task<SensorReading> GetLatestReadingAsync(int domeId);
        Task<bool> SaveChangesAsync();
    }
}
