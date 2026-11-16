using Rovaya.DAL.DTO.Request.Support;
using Rovaya.DAL.DTO.Response.Support;
using System.Security.Claims;

namespace Rovaya.BLL.Service.SupportTickets
{
    public interface ISupportTicketService
    {
        Task<SupportTicketResponse> CreateTicketAsync(ClaimsPrincipal user, CreateTicketRequest request);
        Task<List<SupportTicketResponse>> GetMyTicketsAsync(ClaimsPrincipal user);
        Task<List<SupportTicketResponse>> GetAllTicketsForAdminAsync(ClaimsPrincipal user);
        Task<SupportTicketResponse?> GetTicketByIdAsync(ClaimsPrincipal user, int ticketId);
        Task<SupportTicketResponse> UpdateTicketStatusAsync(ClaimsPrincipal user, UpdateTicketStatusRequest request);
        Task DeleteTicketAsync(ClaimsPrincipal user, int ticketId);
    }
}