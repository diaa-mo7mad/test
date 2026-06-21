using ARGI.DAL.DTO.Response;

namespace ARGI.DAL.DTO.Response
{
    public class IrrigationScheduleResponseDto : BaseResponse
    {
        public int Id { get; set; }
        public int DomeId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsRepeatDaily { get; set; }
        public bool IsExecuted { get; set; }
    }
}
