using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Models
{
    public class SensorReading
    {
        public int Id { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double SoilMoisture { get; set; }
        public double LightIntensity { get; set; }
        public double RainState { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int DomeId { get; set; }
        public virtual Dome Dome { get; set; }

    }
}
