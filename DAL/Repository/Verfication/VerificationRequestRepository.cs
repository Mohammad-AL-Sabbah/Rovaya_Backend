using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Data;
using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Business;

namespace Rovaya.DAL.Repository.Business
{
    public class VerificationRequestRepository : IVerificationRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public VerificationRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VerificationRequest> CreateAsync(VerificationRequest request)
        {
            await _context.VerificationRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<VerificationRequest?> GetByIdAsync(int id)
        {
            return await _context.VerificationRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<VerificationRequest?> GetPendingByUserIdAsync(string userId)
        {
            return await _context.VerificationRequests
                .FirstOrDefaultAsync(r => r.UserId == userId
                    && r.Status == VerificationRequestStatus.Pending);
        }

        public async Task<List<VerificationRequest>> GetAllPendingForAdminAsync()
        {
            return await _context.VerificationRequests
                .Include(r => r.User)
                .Where(r => r.Status == VerificationRequestStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VerificationRequest>> GetByUserIdAsync(string userId)
        {
            return await _context.VerificationRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(VerificationRequest request)
        {
            _context.VerificationRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        // ✅ ✅ ✅ التنفيذ الجديد للأرشيف
        public async Task<List<VerificationRequest>> GetAllForAdminAsync(VerificationRequestStatus? status = null)
        {
            var query = _context.VerificationRequests
                .Include(r => r.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            return await query
                .OrderByDescending(r => r.ReviewedAt ?? r.CreatedAt)
                .ToListAsync();
        }
    }
}