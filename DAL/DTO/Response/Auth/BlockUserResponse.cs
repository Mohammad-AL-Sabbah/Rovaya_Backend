namespace Rovaya.DAL.DTO.Response.Auth
{
    public class BlockUserResponse : BaseResponse
    {
        public string? RemainingBlockTime { get; set; } // لإرجاع المدة المتبقية إذا كان محظوراً مؤقتاً
    }
}