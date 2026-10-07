using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Data;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Identity;

namespace Rovaya.DAL.Repository.Profile
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public ProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.MedicalProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserMedicalProfile?> GetMedicalProfileByUserIdAsync(string userId)
        {
            return await _context.UserMedicalProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == userId);
        }

        public async Task<UserMedicalProfile> CreateMedicalProfileAsync(UserMedicalProfile medicalProfile)
        {
            await _context.UserMedicalProfiles.AddAsync(medicalProfile);
            await _context.SaveChangesAsync();
            return medicalProfile;
        }

        public async Task UpdateMedicalProfileAsync(UserMedicalProfile medicalProfile)
        {
            _context.UserMedicalProfiles.Update(medicalProfile);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMedicalProfileAsync(string userId)
        {
            var medicalProfile = await _context.UserMedicalProfiles
                .FirstOrDefaultAsync(m => m.ApplicationUserId == userId);

            if (medicalProfile != null)
            {
                _context.UserMedicalProfiles.Remove(medicalProfile);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.MedicalProfile)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

    }
}