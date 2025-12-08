using AuthenticationApi;
using AuthenticationApi.Authentications;
using AuthenticationApi.Models;
using AuthenticationApi.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(option => {
    if (string.IsNullOrEmpty(connectionString)) {
        option.UseInMemoryDatabase("AuthDb");
    } else {
        option.UseSqlServer(connectionString);
    }
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.User.RequireUniqueEmail = true;

    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //option.DefaultChallengeScheme = "Bearer";
    //option.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtHandler>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender<IdentityUser>, EmailService>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option => {
    option.SwaggerDoc("v1", new OpenApiInfo() {
        Title = "Auth Demo",
        Version = "v1"
    });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
        In = ParameterLocation.Header,
        Description = "Please enter a token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement() {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger/index.html"));
} else {
    app.Map("/", () => "API is running.");
}

// Seed roles and a default admin
using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    string[] roles = { "Admin", "User" };
    foreach (var role in roles) {
        if (!await roleManager.RoleExistsAsync(role)) {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create default admin if missing
    var adminEmail = builder.Configuration["EmailSettings:UserName"];
    var adminUserName = "Administrator";
    var adminPassword = "Admin123$";

    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null) {
        admin = new IdentityUser {
            UserName = adminUserName,
            Email = adminEmail,
        };

        var create = await userManager.CreateAsync(admin, adminPassword);
        if (create.Succeeded) {
            await userManager.AddToRolesAsync(admin, new[] { "Admin", "User" });
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();