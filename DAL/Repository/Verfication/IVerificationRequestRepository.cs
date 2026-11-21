using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Business;

namespace Rovaya.DAL.Repository.Business
{
    public interface IVerificationRequestRepository
    {
        Task<VerificationRequest> CreateAsync(VerificationRequest request);
        Task<VerificationRequest?> GetByIdAsync(int id);
        Task<VerificationRequest?> GetPendingByUserIdAsync(string userId);
        Task<List<VerificationRequest>> GetAllPendingForAdminAsync();
        Task<List<VerificationRequest>> GetByUserIdAsync(string userId);
        Task UpdateAsync(VerificationRequest request);

        Task<List<VerificationRequest>> GetAllForAdminAsync(VerificationRequestStatus? status = null);
    }
}