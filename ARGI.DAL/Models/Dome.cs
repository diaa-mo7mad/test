using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Models
{
    public  class Dome
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string MacAddress { get; set; }

       
        public string Country { get; set; }     
        public string Governorate { get; set; }  
        public string Neighborhood { get; set; }
       
        public string PlantType { get; set; }
        public string SoilType { get; set; }
        public bool IsAiEnabled { get; set; } = true;
        public double MinTargetMoisture { get; set; } = 30.0;

        // نطاقات مثالية يحددها الذكاء الاصطناعي حسب نوع النبات
        public double OptimalMoistureMin { get; set; } = 50.0;
        public double OptimalMoistureMax { get; set; } = 75.0;
        public double OptimalTempMin { get; set; } = 18.0;
        public double OptimalTempMax { get; set; } = 28.0;
        public double OptimalLightMin { get; set; } = 5000.0;
        public double OptimalLightMax { get; set; } = 10000.0;
        public bool IsPlantProfileCalibrated { get; set; } = false;

        
        public bool IsManualWateringRequested { get; set; } = false;
        public string WateringSource { get; set; } = "Manual"; // Manual, Scheduled, AI
        public int WateringDurationMinutes { get; set; } = 15;  // مدة السقاية المطلوبة للقطعة
        public DateTime? LastPingTime { get; set; }
        public DateTime? LastWateredAt { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<SensorReading> SensorReadings { get; set; } = new HashSet<SensorReading>();
        public virtual ICollection<IrrigationSchedule> IrrigationSchedules { get; set; } = new HashSet<IrrigationSchedule>();
        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
    }
}
