namespace ARGI.DAL.DTO.Response
{
    public class NotificationResponseDto : BaseResponse
    {
        public int Id { get; set; }
        public int DomeId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
