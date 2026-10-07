using Microsoft.AspNetCore.Identity.UI.Services;
using Rovaya.BLL.Service.Audit;
using Rovaya.BLL.Service.Auth;
using Rovaya.BLL.Service.Email;
using Rovaya.BLL.Service.External;
using Rovaya.BLL.Service.Profile;
using Rovaya.BLL.Service.Recommendations;
using Rovaya.BLL.Service.TourismGuide;
using Rovaya.DAL.Repository.Audit;
using Rovaya.DAL.Repository.Profile;
using Rovaya.DAL.Repository.Recommendations;
using Rovaya.DAL.Repository.TourismGuid;
using Rovaya.DAL.Utils;

namespace Rovaya.PL
{
    public static class AppConfigration
    {
        public static void Config(IServiceCollection Services)
        {
            // Dependency Injection
            Services.AddScoped<ITourismGuidRepository, TourismGuidRepository>();
            Services.AddScoped<ITourismGuidService, TourismGuidService>();
            Services.AddScoped<ISeedData, RoleSeedData>();
            Services.AddScoped<ISeedData, UserSeedData>();
            Services.AddScoped<IAuthenticationService, AuthenticationService>();
            Services.AddTransient<IEmailSender, EmailSender>();
            // Profile Services
            Services.AddScoped<IProfileRepository, ProfileRepository>();
            Services.AddScoped<IProfileService, ProfileService>();

            // External Services - Tourism Ministry API
            Services.AddHttpClient<ITourismNewsService, TourismNewsService>();

            // أو إذا كنت تريد إعداد HttpClient بشكل مخصص:
            Services.AddHttpClient("TourismMinistryAPI", client =>
            {
                client.BaseAddress = new Uri("https://tourism.ps/wp-json/wp/v2");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            Services.AddScoped<ITourismNewsService, TourismNewsService>();
            // Audit Services
            Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            Services.AddScoped<IAuditLogService, AuditLogService>();


            // Recommendation Services
            Services.AddScoped<IRecommendationRepository, RecommendationRepository>();
            Services.AddScoped<IRecommendationService, RecommendationService>();


        }
    }
}
