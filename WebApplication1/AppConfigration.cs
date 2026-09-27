using Microsoft.AspNetCore.Identity.UI.Services;
using Rovaya.BLL.Service.Auth;
using Rovaya.BLL.Service.Email;
using Rovaya.BLL.Service.TourismGuide;
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

        }
    }
}
