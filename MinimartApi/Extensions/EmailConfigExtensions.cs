using Microsoft.AspNetCore.Identity;

namespace MinimartApi.Extensions {
    public static class EmailConfigExtensions {
        public static IServiceCollection AddEmailConfig(this IServiceCollection services, IConfiguration config) {

            services.Configure<Services.EmailSettings>(config.GetSection("EmailSettings"));
            services.AddTransient<IEmailSender<IdentityUser>, Services.EmailService>();

            return services;
        }
    }
}
