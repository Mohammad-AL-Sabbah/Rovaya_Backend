using Rovaya.DAL.DTO.Response;

namespace Rovaya.DAL.DTO.Response.Profile
{
    public class AllUsersResponse : BaseResponse
    {
        public List<UserSummaryResponse>? Users { get; set; }
        public int TotalCount { get; set; }
    }
}