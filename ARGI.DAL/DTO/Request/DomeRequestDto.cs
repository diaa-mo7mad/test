using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.DTO.Request
{
    public class DomeRequestDto
    {
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public string Country { get; set; }
        public string Governorate { get; set; }
        public string Neighborhood { get; set; }
        public string PlantType { get; set; }
        public string SoilType { get; set; }
        public bool IsAiEnabled { get; set; } = true;
        public double MinTargetMoisture { get; set; } = 30.0;
    }
}
