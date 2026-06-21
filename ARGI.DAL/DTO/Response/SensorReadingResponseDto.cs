using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.DTO.Response
{
    public class SensorReadingResponseDto:BaseResponse
    {
        public int Id { get; set; }
        public int DomeId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double SoilMoisture { get; set; }
        public double LightIntensity { get; set; }
        public double RainState { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
