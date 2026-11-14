using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Rovaya.BLL.Service.Auth;
using Rovaya.BLL.Service.Email;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.Data;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Repository.TourismGuid;
using Rovaya.DAL.Utils;
using Rovaya.PL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. تسجيل الخدمات الأساسية
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 7;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            AppConfigration.Config(builder.Services);

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(5);
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("StrictEmailPolicy", opt =>
                {
                    opt.PermitLimit = 2;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            // ==========================================
            // ✅ إعدادات CORS النهائية (تسمح للتطوير والإنتاج والموبايل)
            // ==========================================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowRovayaApps", policy =>
                {
                    policy.WithOrigins(
                        "http://localhost:5173",          // تطوير الويب (Vite)
                        "https://rovaya.onrender.com",     // إنتاج الويب
                        "http://localhost:3000",           // تطوير بديل
                        "capacitor://localhost",           // للموبايل (إذا كنت تستخدم Capacitor)
                        "http://localhost"    ,
                        "https://rovaya.vercel.app"// للموبايل أحياناً
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // ضروري جداً لمرور التوكنات
                });
            });

            // ==========================================
            // بناء التطبيق (هنا ينتهي تسجيل الخدمات)
            // ==========================================
            var app = builder.Build();

            // ==========================================
            // ✅ ترتيب الـ Middleware (مهم جداً لا تغيره)
            // ==========================================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // السماح بقراءة الصور من مجلد wwwroot
            app.UseStaticFiles();

            // ✅ تفعيل CORS هنا (يجب أن يكون قبل Authentication)
            app.UseCors("AllowRovayaApps");

            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            // 3. Database Seeding
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var roleSeeders = services.GetServices<ISeedData>().OfType<RoleSeedData>().ToList();
                    foreach (var seeder in roleSeeders) await seeder.DataSeed();

                    var userSeeders = services.GetServices<ISeedData>().OfType<UserSeedData>().ToList();
                    foreach (var seeder in userSeeders) await seeder.DataSeed();

                    Console.WriteLine("✅ Data Seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Seeding error: {ex.Message}");
                }
            }

            app.MapControllers();
            app.Run();
        }
    }
}