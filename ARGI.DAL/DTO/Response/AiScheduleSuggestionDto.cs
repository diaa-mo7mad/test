namespace ARGI.DAL.DTO.Response
{
    public class AiScheduleSuggestionDto
    {
        public string Time { get; set; }       
        public int DurationMinutes { get; set; }
        public string Reason { get; set; }
        public bool IsRepeatDaily { get; set; } = true;
    }
}
