using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.Data;
using Rovaya.DAL.Repository.TourismGuid;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Services Configuration (قبل builder.Build)
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Swagger Setup
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Dependency Injection
            builder.Services.AddScoped<ITourismGuidRepository, TourismGuidRepository>();
            builder.Services.AddScoped<ITourismGuidService, TourismGuidService>();

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

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}