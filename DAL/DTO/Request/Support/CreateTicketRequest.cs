using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Rovaya.DAL.DTO.Request.Support
{
    public class CreateTicketRequest
    {
        [Required(ErrorMessage = "عنوان المشكلة مطلوب")]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "شرح المشكلة مطلوب")]
        [MaxLength(2000)]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "رقم الهاتف مطلوب للتواصل")]
        [Phone(ErrorMessage = "رقم هاتف غير صالح")]
        public string PhoneNumber { get; set; } = null!;

        public List<IFormFile>? Images { get; set; }
    }
}