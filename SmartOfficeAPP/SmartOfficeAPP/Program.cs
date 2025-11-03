using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartOfficeAPP.Authorization;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.Hubs;
using SmartOfficeAPP.Helpers;
using SmartOfficeAPP.Models.Identity;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Repositories.Repos;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.Services.ServicesImpl;
using SmartOfficeAPP.Middleware;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

// ✅ Redis + OutputCache Configuration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? builder.Configuration["Redis:ConnectionString"];

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    // Ajouter Redis pour la mise en cache
    builder.Services.AddStackExchangeRedisCache(options =>
    {
   options.Configuration = redisConnectionString;
     options.InstanceName = "SmartOfficeAPI";
    });

    // Configurer OutputCache avec Redis
  builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy("DefaultPolicy", policy =>
   {
            policy.Expire(TimeSpan.FromMinutes(5));
    policy.SetVaryByQuery("fresh", "limit", "offset");
            policy.Tag("api-data");
    });

        options.AddPolicy("DeviceData", policy =>
        {
   policy.Expire(TimeSpan.FromMinutes(2));
 policy.SetVaryByQuery("fresh", "deviceId");
    policy.Tag("device-data");
        });

  options.AddPolicy("TelemetryData", policy =>
        {
            policy.Expire(TimeSpan.FromMinutes(1));
   policy.SetVaryByQuery("fresh", "peripheriqueId", "startDate", "endDate");
     policy.Tag("telemetry-data");
      });
    });

    builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Redis configuré avec OutputCache pour : {RedisConnectionString}", redisConnectionString);
}
else
{
    // Fallback vers mémoire cache
  builder.Services.AddMemoryCache();
    builder.Services.AddOutputCache(options =>
    {
   options.AddPolicy("DefaultPolicy", policy =>
     {
 policy.Expire(TimeSpan.FromMinutes(3));
       policy.SetVaryByQuery("fresh", "limit", "offset");
  });

        options.AddPolicy("DeviceData", policy =>
        {
  policy.Expire(TimeSpan.FromMinutes(1));
            policy.SetVaryByQuery("fresh", "deviceId");
        });

        options.AddPolicy("TelemetryData", policy =>
        {
   policy.Expire(TimeSpan.FromSeconds(30));
          policy.SetVaryByQuery("fresh", "peripheriqueId", "startDate", "endDate");
     });
    });

    builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Information);
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogWarning("Redis non configuré, utilisation du cache mémoire. Pour activer Redis : dotnet user-secrets set \"Redis:ConnectionString\" \"localhost:6379\"");
}

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// SignalR avec configuration avancée
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
  options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
});

// Auth JWT avec support SignalR
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!)),
        ClockSkew = TimeSpan.Zero
    };
    
    // Configuration pour SignalR
    options.Events = new JwtBearerEvents
    {
   OnAuthenticationFailed = context =>
    {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
  logger.LogError(context.Exception, "Échec d'authentification JWT: {Error}", context.Exception.Message);
            return Task.CompletedTask;
     },
  OnChallenge = context =>
 {
    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
 logger.LogWarning("Challenge JWT déclenché: {Error}", context.Error);
         return Task.CompletedTask;
        },
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

// CORS avec support SignalR
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowFrontend", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins("https://localhost:7152", "http://localhost:5249", "http://localhost:3000", "https://localhost:3000")
   .AllowCredentials());
});

// Enregistrement Authorization avec handlers
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, ResourceAccessHandler>();

// Repos & services
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<IPeripheriqueLogiqueService, PeripheriqueLogiqueService>();
builder.Services.AddScoped<IPeripheriqueLogiqueRepository, PeripheriqueLogiqueRepository>();
builder.Services.AddScoped<IDevicePeripheralRepository, DevicePeripheralRepository>();
builder.Services.AddScoped<IDevicePeripheralService, DevicePeripheralService>();
builder.Services.AddScoped<IDeviceLogRepository, DeviceLogRepository>();
builder.Services.AddScoped<IDeviceLogService, DeviceLogService>();
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
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
builder.Services.AddScoped<DataSeedingService>();

// ✅ Service de synchronisation DeviceSyncService
builder.Services.AddScoped<IDeviceSyncService, DeviceSyncService>();

// Nouveau service SignalR
builder.Services.AddScoped<ISignalRService, SignalRService>();

// Nouveau service de statistiques de consommation
builder.Services.AddScoped<IConsommationStatsService, ConsommationStatsService>();

// 🔧 Nouveaux repositories et service pour fonctionnalités firmware
builder.Services.AddScoped<IDeviceMetricsRepository, DeviceMetricsRepository>();
builder.Services.AddScoped<IRfidEventRepository, RfidEventRepository>();
builder.Services.AddScoped<ISmokeAlarmEventRepository, SmokeAlarmEventRepository>();
builder.Services.AddScoped<IDeviceStatusService, DeviceStatusService>();

builder.Services.AddHostedService<SmartOfficeAPP.Services.Background.TelemetrieIngestionWorker>();
builder.Services.AddHostedService<SmartOfficeAPP.Services.Background.LogIngestionWorker>();
// ✅ Ajouter le worker de synchronisation périodique
builder.Services.AddHostedService<SmartOfficeAPP.Services.Background.DeviceSyncWorker>();

// ✅ Configuration JSON pour camelCase (compatible firmware ESP32)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
      // Utiliser camelCase pour les noms de propriétés JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
     
        // Ignorer les valeurs null dans la sérialisation
 options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    
// Permettre la lecture de nombres depuis des chaînes
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        
   // Configuration pour gérer les références circulaires (si nécessaire)
     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        
        // Autres options utiles
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Configuration d'AutoMapper avec licence
var autoMapperLicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
if (!string.IsNullOrEmpty(autoMapperLicenseKey))
{
    Environment.SetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY", autoMapperLicenseKey);
}

builder.Services.AddAutoMapper(typeof(MappingProfile));

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
        PermitLimit = 100,
        Window = TimeSpan.FromSeconds(10),
 QueueLimit = 0
    })
));

var app = builder.Build();

// Seeding des données initiales
using (var scope = app.Services.CreateScope())
{
    var seedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
    await seedingService.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
}

// ✅ MIDDLEWARE NUCLÉAIRE ACTIVÉ: Force camelCase pour TOUTES les réponses JSON
// Ce middleware intercepte TOUTES les réponses et convertit le JSON en camelCase
app.UseMiddleware<CamelCaseEnforcerMiddleware>();

app.UseCors("AllowFrontend");

// ✅ Activer OutputCache
app.UseOutputCache();

// Mapping du hub SignalR
app.MapHub<NotificationHub>("/notificationhub");

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();
