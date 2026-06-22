using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.DTO.Response
{
    public class DomeResponseDto : BaseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public string Country { get; set; }
        public string Governorate { get; set; }
        public string Neighborhood { get; set; }
        public string PlantType { get; set; }
        public string SoilType { get; set; }
        public bool IsAiEnabled { get; set; }
        public double MinTargetMoisture { get; set; }
        public double OptimalMoistureMin { get; set; }
        public double OptimalMoistureMax { get; set; }
        public double OptimalTempMin { get; set; }
        public double OptimalTempMax { get; set; }
        public double OptimalLightMin { get; set; }
        public double OptimalLightMax { get; set; }
        public bool IsManualWateringRequested { get; set; }
        public string WateringSource { get; set; }
        public DateTime? LastPingTime { get; set; }
        public DateTime? LastWateredAt { get; set; }
        public string UserId { get; set; }
    }
}
