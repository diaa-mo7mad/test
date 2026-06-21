namespace ARGI.DAL.DTO.Response
{
    public class PlantProfileDto : BaseResponse
    {
        public double OptimalMoistureMin { get; set; }
        public double OptimalMoistureMax { get; set; }
        public double OptimalTempMin { get; set; }
        public double OptimalTempMax { get; set; }
        public double OptimalLightMin { get; set; }
        public double OptimalLightMax { get; set; }
        public string PlantType { get; set; }
        public string Notes { get; set; }
    }
}
