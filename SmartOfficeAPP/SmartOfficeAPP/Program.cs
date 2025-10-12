using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.Hubs;
using SmartOfficeAPP.Models.Identity;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Repositories.Repos;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.Services.ServicesImpl;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSignalR();

// Auth JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Support JWT pour SignalR (query access_token)
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// CORS
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins("https://localhost:5173", "http://localhost:5173") // adapter si besoin
        .AllowCredentials());
});

// Repos & services
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<IPeripheriqueLogiqueService, PeripheriqueLogiqueService>();
builder.Services.AddScoped<IPeripheriqueLogiqueRepository, PeripheriqueLogiqueRepository>();
builder.Services.AddScoped<ICommandeRepository, CommandeRepository>();
builder.Services.AddScoped<ICommandeService, CommandeService>();
builder.Services.AddScoped<IIotHubService, IotHubService>();
builder.Services.AddScoped<ITelemetrieRepository, TelemetrieRepository>();
builder.Services.AddScoped<ITelemetrieService, TelemetrieService>();
builder.Services.AddScoped<IDroitAccesRepository, DroitAccesRepository>();
builder.Services.AddScoped<IDroitAccesService, DroitAccesService>();
builder.Services.AddScoped<IJournalAuditRepository, JournalAuditRepository>();
builder.Services.AddScoped<IJournalAuditService, JournalAuditService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IIncidentTicketService, IncidentTicketService>();
builder.Services.AddScoped<ISuggestionRepository, SuggestionRepository>();
builder.Services.AddHostedService<SmartOfficeAPP.Services.Background.TelemetrieIngestionWorker>();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SmartOffice API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Bearer. Exemple: 'Bearer {token}'",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks().AddNpgSql(connectionString);
builder.Services.AddRateLimiter(o => o.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(
    _ => System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter("global", _ => new()
    {
        PermitLimit = 100, // 100 req / 10s
        Window = TimeSpan.FromSeconds(10),
        QueueLimit = 0
    })
));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error"); // ajouter un endpoint ProblemDetails si souhaité
}

app.UseCors("AllowFrontend");
app.MapHub<NotificationHub>("/notificationhub");

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
