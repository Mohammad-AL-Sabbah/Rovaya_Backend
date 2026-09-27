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
                options.Password.RequireDigit  = true;
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

            // Swagger Setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            AppConfigration.Config(builder.Services);
        
            // Email Token Expiry (5 Minutes)
            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(5);
            });

            // Rate Limiter Service Configuration (يجب أن تكون قبل builder.Build)
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("StrictEmailPolicy", opt =>
                {
                    opt.PermitLimit = 2; // يسمح بطلبين فقط
                    opt.Window = TimeSpan.FromMinutes(1); // كل دقيقة واحدة
                    opt.QueueLimit = 0; // بدون انتظار في الطابور
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            // Authentication & JWT Setup
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

            // CORS Policy Setup
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // =========================================================
            // Build the Application (هنا يتوقف إضافة الخدمات للـ DI)
            // =========================================================
            var app = builder.Build();
            // =========================================================

            // 2. Middleware Pipeline Configuration
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // Static Files for image access from wwwroot
            app.UseStaticFiles();

            // CORS Middleware
            app.UseCors("AllowAll");

            // Rate Limiter Middleware
            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            // 3. Database Seeding
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // الخطوة الأولى: إنشاء الأدوار
                    var roleSeeders = services.GetServices<ISeedData>()
                        .OfType<RoleSeedData>()
                        .ToList();

                    foreach (var seeder in roleSeeders)
                    {
                        await seeder.DataSeed();
                    }

                    // الخطوة الثانية: إنشاء المستخدمين
                    var userSeeders = services.GetServices<ISeedData>()
                        .OfType<UserSeedData>()
                        .ToList();

                    foreach (var seeder in userSeeders)
                    {
                        await seeder.DataSeed();
                    }

                    Console.WriteLine("✅ Data Seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ An error occurred while seeding the database: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }

            app.MapControllers();

            app.Run();
        }
    }
}