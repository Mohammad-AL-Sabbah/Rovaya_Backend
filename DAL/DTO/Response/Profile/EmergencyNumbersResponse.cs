using Rovaya.DAL.DTO.Response;

namespace Rovaya.DAL.DTO.Response.Profile
{
    public class EmergencyNumbersResponse : BaseResponse
    {
        public List<string>? Numbers { get; set; }
    }
}