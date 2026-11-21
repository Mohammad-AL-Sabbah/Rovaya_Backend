using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Data;
using Rovaya.DAL.Models.Business;

namespace Rovaya.DAL.Repository.Business
{
    public class BusinessPlaceRepository : IBusinessPlaceRepository
    {
        private readonly ApplicationDbContext _context;

        public BusinessPlaceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BusinessPlace> CreateAsync(BusinessPlace place)
        {
            await _context.BusinessPlaces.AddAsync(place);
            await _context.SaveChangesAsync();
            return place;
        }

        public async Task<BusinessPlace?> GetByIdAsync(int id)
        {
            return await _context.BusinessPlaces
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<BusinessPlace>> GetByOwnerIdAsync(string ownerId)
        {
            return await _context.BusinessPlaces
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BusinessPlace>> GetAllActiveAsync()
        {
            return await _context.BusinessPlaces
                .Include(p => p.Owner)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(BusinessPlace place)
        {
            _context.BusinessPlaces.Update(place);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var place = await _context.BusinessPlaces.FirstOrDefaultAsync(p => p.Id == id);
            if (place != null)
            {
                _context.BusinessPlaces.Remove(place);
                await _context.SaveChangesAsync();
            }
        }
    }
}