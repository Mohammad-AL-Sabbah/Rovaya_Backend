using Rovaya.DAL.Models.Support;

namespace Rovaya.DAL.Repository.Support
{
    public interface ISupportTicketRepository
    {
        Task<SupportTicket> CreateAsync(SupportTicket ticket);
        Task<SupportTicket?> GetByIdAsync(int id);
        Task<List<SupportTicket>> GetByUserIdAsync(string userId);
        Task<List<SupportTicket>> GetAllForAdminAsync(); // للموظفين والإدارة
        Task<SupportTicket> UpdateStatusAsync(SupportTicket ticket);
        Task DeleteAsync(int id);
    }
}