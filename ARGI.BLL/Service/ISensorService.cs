using ARGI.DAL.DTO.Request;
using ARGI.DAL.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARGI.BLL.Service
{
    public interface ISensorService
    {
        Task<BaseResponse> SaveReadingAsync(SensorReadingRequestDto request);
        Task<IEnumerable<SensorReadingResponseDto>> GetHistoricalDataAsync(int domeId, string userId);
        Task<object> GetEsp32CommandAsync(string macAddress);
        Task<SensorReadingResponseDto> GetLatestReadingAsync(int domeId, string userId);
    }
}
