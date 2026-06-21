using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.DTO.Request
{
    public class SensorReadingRequestDto
    {
        public string MacAddress { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double SoilMoisture { get; set; }
        public double LightIntensity { get; set; }
        public double RainState { get; set; }
        public DateTime? Timestamp { get; set; } // اختياري — للبيانات التاريخية
    }
}
