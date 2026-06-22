namespace ARGI.DAL.DTO.Response
{
    public class IrrigationDecisionDto
    {
        public bool ShouldWater { get; set; }
        public int DurationMinutes { get; set; } = 15;
        public double TargetUpperMoisture { get; set; } 
        public string Reason { get; set; }
    }
}
