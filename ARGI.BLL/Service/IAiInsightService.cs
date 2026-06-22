using ARGI.DAL.DTO.Response;

namespace ARGI.BLL.Service
{
    public interface IAiInsightService
    {
        Task<List<AiInsightDto>> GenerateInsightsAsync(int domeId, string userId);
        Task<PlantProfileDto> CalibrateProfileAsync(int domeId, string userId);
        
        Task<IrrigationDecisionDto> EvaluateIrrigationAsync(int domeId);
        Task<double> CalculateHealthScoreAsync(int domeId);
        Task<List<AiScheduleSuggestionDto>> SuggestScheduleAsync(int domeId, string userId);
    }
}
