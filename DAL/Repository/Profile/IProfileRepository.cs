using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Identity;

namespace Rovaya.DAL.Repository.Profile
{
    public interface IProfileRepository
    {
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(ApplicationUser user);

        Task<UserMedicalProfile?> GetMedicalProfileByUserIdAsync(string userId);
        Task<UserMedicalProfile> CreateMedicalProfileAsync(UserMedicalProfile medicalProfile);
        Task UpdateMedicalProfileAsync(UserMedicalProfile medicalProfile);
        Task DeleteMedicalProfileAsync(string userId);
        Task<List<ApplicationUser>> GetAllUsersAsync();
    }
}