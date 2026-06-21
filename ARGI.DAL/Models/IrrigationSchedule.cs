using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.DAL.Models
{
    public  class IrrigationSchedule
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; } 
        public int DurationMinutes { get; set; } 
        public bool IsRepeatDaily { get; set; } = true;
        public bool IsExecuted { get; set; } = false;
        public int DomeId { get; set; }
        public virtual Dome Dome { get; set; }
    }
}
