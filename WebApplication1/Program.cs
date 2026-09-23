using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rovaya.BLL.Service.Auth;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.Data;
using Rovaya.DAL.Models.Auth;
using Rovaya.DAL.Repository.TourismGuid;
using Rovaya.DAL.Utils;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

          
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



            //Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            // Swagger Setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Dependency Injection
            builder.Services.AddScoped<ITourismGuidRepository, TourismGuidRepository>();
            builder.Services.AddScoped<ITourismGuidService, TourismGuidService>();
            builder.Services.AddScoped<ISeedData, RoleSeedData>();
            builder.Services.AddScoped<ISeedData, UserSeedData>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();



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
            // Build the Application
            var app = builder.Build();
            // =========================================================

            // 2. Middleware Pipeline (بعد builder.Build)
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
            app.UseAuthentication();

            app.UseAuthorization();
            // 2. Database Seeding (بالترتيب الصحيح: الأدوار أولاً، ثم المستخدمين)
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