using Microsoft.AspNetCore.Identity;
using MinimartApi.Db.Models;

namespace MinimartApi.Extensions {
    public static class EmailConfigExtensions {
        public static IServiceCollection AddEmailConfig(this IServiceCollection services, IConfiguration config) {

            services.Configure<Services.EmailSettings>(config.GetSection("EmailSettings"));
            services.AddTransient<IEmailSender<User>, Services.EmailService>();

            return services;
        }
    }
}
