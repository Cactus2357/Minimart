using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using MinimartApi.Authentications;
using MinimartApi.Extensions;
using MinimartApi.Models;

//static IEdmModel GetEdmModel() {
//    var builder = new ODataConventionModelBuilder();

//    builder.EntitySet<Category>("Categories");
//    builder.EntitySet<Product>("Products");
//    builder.EntitySet<Customer>("Customers");
//    builder.EntitySet<Sale>("Sales");
//    builder.EntitySet<SaleItem>("SaleItems");
//    builder.EntitySet<Supplier>("Suppliers");
//    builder.EntitySet<PurchaseOrder>("PurchaseOrders");
//    builder.EntitySet<PurchaseOrderItem>("PurchaseOrderItems");
//    builder.EntitySet<Stock>("Stocks");
//    builder.EntitySet<StockTransaction>("StockTransactions");

//    // builder.EntitySet<User>("Users");

//    return builder.GetEdmModel();
//}

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(option => {
    option.UseSqlServer(connectionString);
});

builder.Services.AddJwtConfig(builder.Configuration);
builder.Services.AddEmailConfig(builder.Configuration);
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddMemoryCache();


builder.Services.AddAuthorization();
//builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<JwtHandler>();

//builder.Services.AddControllers().AddOData(opt => {
//    opt.AddRouteComponents("odata", GetEdmModel())
//       .Filter()
//       .Select()
//       .Expand()
//       .OrderBy()
//       .SetMaxTop(100)
//       .Count()
//       .SkipToken();
//});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option => {
    option.SwaggerDoc("v1", new OpenApiInfo() {
        Title = "Minimart",
        Version = "v1"
    });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
        In = ParameterLocation.Header,
        Description = "Please enter access token",
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

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger/index.html"))
        .ExcludeFromDescription();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
