using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Data;
using Rovaya.DAL.Models.Support;

namespace Rovaya.DAL.Repository.Support
{
    public class SupportTicketRepository : ISupportTicketRepository
    {
        private readonly ApplicationDbContext _context;

        public SupportTicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupportTicket> CreateAsync(SupportTicket ticket)
        {
            await _context.SupportTickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<SupportTicket?> GetByIdAsync(int id)
        {
            return await _context.SupportTickets
                .Include(t => t.User)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<SupportTicket>> GetByUserIdAsync(string userId)
        {
            return await _context.SupportTickets
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SupportTicket>> GetAllForAdminAsync()
        {
            return await _context.SupportTickets
                .Include(t => t.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupportTicket> UpdateStatusAsync(SupportTicket ticket)
        {
            _context.SupportTickets.Update(ticket);
            await _context.SaveChangesAsync();
            return ticket;
        }
        public async Task DeleteAsync(int id)
        {
            var ticket = await _context.SupportTickets.FindAsync(id);
            if (ticket != null)
            {
                _context.SupportTickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }
    }
}