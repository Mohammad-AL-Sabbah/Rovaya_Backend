using Microsoft.AspNetCore.Identity;
using Rovaya.DAL.Enums;
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

        public string? CodeResetPassword { get; set; }
        public DateTime? CodeResetPasswordExpiration { get; set; }

        // لتتبع وقت آخر إرسال لرسالة التفعيل للايميل لمنع الإرسال المتكرر

        public DateTime? LastConfirmationEmailSentAt { get; set; }


        public AccountStatus Status { get; set; } = AccountStatus.Active;

        public string? BlockReason { get; set; }

        public DateTime? BlockExpirationDate { get; set; }



    }
}