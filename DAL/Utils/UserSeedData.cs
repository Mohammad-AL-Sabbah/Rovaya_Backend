using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rovaya.DAL.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rovaya.DAL.Utils
{
    public class UserSeedData : ISeedData
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSeedData(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task DataSeed()
        {
            if (!await _userManager.Users.AnyAsync())
            {
                const string defaultPassword = "Password@123";

                var usersToSeed = new List<(string FirstName, string LastName, string Email, string Country, string Role, string Phone)>
                {
                    // ================= 5 عملاء / سياح (Customers) =================
                    ("أحمد", "خليل", "ahmad.khalil@rovaya.com", "الأردن", "Customer", "+962790000001"),
                    ("سارة", "ناصر", "sara.nasser@rovaya.com", "مصر", "Customer", "+201000000002"),
                    ("عمر", "فاروق", "omar.farouq@rovaya.com", "السعودية", "Customer", "+966500000003"),
                    ("ليلى", "حسن", "layla.hassan@rovaya.com", "الإمارات", "Customer", "+971500000004"),
                    ("يوسف", "منصور", "youssef.mansour@rovaya.com", "فلسطين", "Customer", "+970599000005"),

                    // ================= 3 مدراء محتوى (Admins) =================
                    ("طارق", "زياد", "tariq.zayid@rovaya.com", "فلسطين", "Admin", "+970599000006"),
                    ("نور", "الدين", "noor.aldin@rovaya.com", "فلسطين", "Admin", "+970599000007"),
                    ("هدى", "قاسم", "huda.qasem@rovaya.com", "فلسطين", "Admin", "+970599000008"),

                    // ================= 2 مدراء فائقين (Super Admins) =================
                    ("محمد", "صباح", "mohammad.sabbah@rovaya.com", "فلسطين", "SuperAdmin", "+970599000009"),
                    ("رانيا", "خالدي", "rania.khalidi@rovaya.com", "فلسطين", "SuperAdmin", "+970599000010")
                };

                foreach (var userData in usersToSeed)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userData.Email,
                        FirstName = userData.FirstName,
                        LastName = userData.LastName,
                        Email = userData.Email,
                        Country = userData.Country,
                        PhoneNumber = userData.Phone, // الآن يعمل لأن النوع string
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        AdditionalPhoneNumbers = new List<string>(),
                        EmergencyNumbers = new List<string>()
                    };

                    var result = await _userManager.CreateAsync(user, defaultPassword);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, userData.Role);
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        Console.WriteLine($"فشل في إنشاء المستخدم {userData.Email}: {errors}");
                    }
                }
            }
        }
    }
}