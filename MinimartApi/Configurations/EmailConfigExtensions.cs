using Microsoft.AspNetCore.Identity;
using MinimartApi.Db.Models;

namespace MinimartApi.Configurations
{
    public static class EmailConfigExtensions
    {
        public static IServiceCollection AddEmailConfig(this IServiceCollection services, IConfiguration config)
        {

            services.Configure<EmailOptions>(config.GetSection("EmailOptions"));
            services.AddTransient<IEmailSender<User>, Services.EmailService>();

            return services;
        }
    }
}
