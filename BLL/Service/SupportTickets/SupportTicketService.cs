using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Rovaya.BLL.Service.Audit;
using Rovaya.DAL.DTO.Request.Support;
using Rovaya.DAL.DTO.Response.Support;
using Rovaya.DAL.Enums;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Models.Support;
using Rovaya.DAL.Repository.Support;
using System.Security.Claims;
using System.Text.Json;

namespace Rovaya.BLL.Service.SupportTickets
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _ticketRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAuditLogService _auditLogService;

        public SupportTicketService(
            ISupportTicketRepository ticketRepository,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IAuditLogService auditLogService)
        {
            _ticketRepository = ticketRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _auditLogService = auditLogService;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return userId != null ? await _userManager.FindByIdAsync(userId) : null;
        }

        public async Task<SupportTicketResponse> CreateTicketAsync(ClaimsPrincipal user, CreateTicketRequest request)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) throw new Exception("المستخدم غير موجود");

            var imageUrls = new List<string>();
            if (request.Images != null && request.Images.Any())
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tickets");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                foreach (var file in request.Images)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    imageUrls.Add($"/uploads/tickets/{uniqueFileName}");
                }
            }

            var ticket = new SupportTicket
            {
                UserId = currentUser.Id,
                Title = request.Title,
                Description = request.Description,
                PhoneNumber = request.PhoneNumber,
                ImageUrlsJson = JsonSerializer.Serialize(imageUrls),
                Status = TicketStatus.Sent,
                CreatedByUserId = currentUser.Id
            };

            var createdTicket = await _ticketRepository.CreateAsync(ticket);

            // ✅ استدعاء صحيح باستخدام Named Parameters
            await _auditLogService.LogActionAsync(
                user: user,
                action: "CreateTicket",
                targetUserId: null,
                targetUserName: $"Ticket #{createdTicket.Id}",
                details: $"أنشأ تذكرة دعم جديدة بعنوان: {request.Title}",
                reason: "طلب دعم فني من المستخدم"
            );

            return MapToResponse(createdTicket);
        }

        public async Task<List<SupportTicketResponse>> GetMyTicketsAsync(ClaimsPrincipal user)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) throw new Exception("المستخدم غير موجود");

            var tickets = await _ticketRepository.GetByUserIdAsync(currentUser.Id);
            return tickets.Select(MapToResponse).ToList();
        }

        public async Task<List<SupportTicketResponse>> GetAllTicketsForAdminAsync(ClaimsPrincipal user)
        {
            var tickets = await _ticketRepository.GetAllForAdminAsync();
            return tickets.Select(MapToResponse).ToList();
        }

        public async Task<SupportTicketResponse> UpdateTicketStatusAsync(ClaimsPrincipal user, UpdateTicketStatusRequest request)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) throw new Exception("المستخدم غير موجود");

            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId);
            if (ticket == null) throw new Exception("التذكرة غير موجودة");

            var oldStatus = ticket.Status;
            ticket.Status = request.NewStatus;
            ticket.AdminNotes = request.AdminNotes;
            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.UpdatedByUserId = currentUser.Id;

            var updatedTicket = await _ticketRepository.UpdateStatusAsync(ticket);

            // ✅ استدعاء صحيح باستخدام Named Parameters
            await _auditLogService.LogActionAsync(
                user: user,
                action: "UpdateTicketStatus",
                targetUserId: ticket.UserId,
                targetUserName: $"Ticket #{ticket.Id}",
                details: $"غيّر حالة التذكرة من {oldStatus} إلى {request.NewStatus}",
                reason: request.AdminNotes ?? "تحديث من الدعم الفني"
            );

            return MapToResponse(updatedTicket);
        }

        private SupportTicketResponse MapToResponse(SupportTicket ticket)
        {
            var imageUrls = string.IsNullOrEmpty(ticket.ImageUrlsJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(ticket.ImageUrlsJson) ?? new List<string>();

            // بناء روابط كاملة للصور
            var baseUrl = "https://rovayabackend.runasp.net";
            var fullImageUrls = imageUrls
                .Select(img => img.StartsWith("http") ? img : $"{baseUrl}{img}")
                .ToList();

            return new SupportTicketResponse
            {
                Id = ticket.Id,
                UserName = ticket.User != null
                    ? $"{ticket.User.FirstName} {ticket.User.LastName}".Trim()
                    : "مستخدم محذوف",
                UserPhone = ticket.PhoneNumber,
                Title = ticket.Title,
                Description = ticket.Description,
                ImageUrls = fullImageUrls,
                Status = ticket.Status,
                AdminNotes = ticket.AdminNotes,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt
            };
        }


        public async Task<SupportTicketResponse?> GetTicketByIdAsync(ClaimsPrincipal user, int ticketId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            return ticket == null ? null : MapToResponse(ticket);
        }

        public async Task DeleteTicketAsync(ClaimsPrincipal user, int ticketId)
        {
            var currentUser = await GetCurrentUserAsync(user);
            if (currentUser == null) throw new Exception("المستخدم غير موجود");

            // ✅ التحقق من الصلاحية (Admin أو SuperAdmin فقط)
            if (!user.IsInRole("Admin") && !user.IsInRole("SuperAdmin"))
                throw new Exception("لا تملك صلاحية حذف التذاكر");

            // 1. جلب بيانات التذكرة أولاً
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null) throw new Exception("التذكرة غير موجودة");

            // ✅ حفظ البيانات في متغيرات محلية قبل الحذف لضمان استخدامها في الـ Audit
            var ticketTitle = ticket.Title;
            var ticketUserId = ticket.UserId;

            // 2. حذف الصور المرفقة من السيرفر (إن وجدت)
            if (!string.IsNullOrEmpty(ticket.ImageUrlsJson))
            {
                var imageUrls = JsonSerializer.Deserialize<List<string>>(ticket.ImageUrlsJson) ?? new List<string>();
                foreach (var img in imageUrls)
                {
                    var fileName = Path.GetFileName(img);
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tickets", fileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
            }

            // 3. حذف التذكرة من قاعدة البيانات
            await _ticketRepository.DeleteAsync(ticketId);

            // 4. ✅ تسجيل العملية في سجل التدقيق (Audit Log) باستخدام البيانات المحفوظة
            await _auditLogService.LogActionAsync(
                user: user,
                action: "DeleteTicket",
                targetUserId: ticketUserId,
                targetUserName: $"تذكرة دعم #{ticketId}",
                details: $"تم حذف تذكرة الدعم الفني بعنوان: {ticketTitle}",
                reason: "إجراء إداري من لوحة التحكم"
            );
        }



    }
}