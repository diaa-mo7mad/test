namespace ARGI.DAL.DTO.Request
{
    public class IrrigationScheduleRequestDto
    {
        public int DomeId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsRepeatDaily { get; set; } = true;
    }
}
