using Microsoft.AspNetCore.Identity;
using Rovaya.DAL.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rovaya.DAL.Models.Auth
{
    public class ApplicationUser : IdentityUser
    {
        // البيانات الأساسية
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Country { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<string> AdditionalPhoneNumbers { get; set; } = new();

        // بيانات الملف الشخصي
        public string? ProfileImageUrl { get; set; }

        // أرقام الطوارئ
        public List<string> EmergencyNumbers { get; set; } = new();

        // الملف الطبي
        public UserMedicalProfile? MedicalProfile { get; set; }
    }
}