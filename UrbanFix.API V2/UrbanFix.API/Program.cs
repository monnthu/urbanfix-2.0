using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UrbanFix.API.Authorization;
using UrbanFix.API.Data;
using UrbanFix.API.Extensions;
using UrbanFix.API.Options;
using UrbanFix.API.Services;

if (args.Contains("--apply-auth-sql", StringComparer.OrdinalIgnoreCase))
{
    var bootstrapConfig = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    Environment.Exit(await AuthSqlBootstrap.ApplyAuthTriggerAsync(bootstrapConfig));
}

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}

builder.Configuration.ValidateSupabaseConfiguration();

var supabaseSection = builder.Configuration.GetSection(SupabaseOptions.SectionName);
var supabaseUrl = supabaseSection["Url"]!;

builder.Services.Configure<SupabaseOptions>(supabaseSection);
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.AddControllers();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 25 * 1024 * 1024;
});

builder.Services.AddHttpClient<ISupabaseStorageService, SupabaseStorageService>();
builder.Services.AddHttpClient<ISupabaseAdminAuthService, SupabaseAdminAuthService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

builder.Services.AddDbContext<ReportContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var supabaseIssuer = $"{supabaseUrl.TrimEnd('/')}/auth/v1";
var supabaseJwtSecret = supabaseSection["JwtSecret"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;
        // Proyectos nuevos de Supabase firman con ES256/RS256 vía JWKS, no con el legacy JWT secret.
        options.MetadataAddress = $"{supabaseIssuer}/.well-known/openid-configuration";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = supabaseIssuer,
            ValidAudience = "authenticated",
            ValidAlgorithms =
            [
                SecurityAlgorithms.EcdsaSha256,
                SecurityAlgorithms.RsaSha256,
                SecurityAlgorithms.HmacSha256
            ],
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        if (!string.IsNullOrWhiteSpace(supabaseJwtSecret)
            && !supabaseJwtSecret.Contains("YOUR_", StringComparison.OrdinalIgnoreCase))
        {
            options.TokenValidationParameters.IssuerSigningKey =
                SupabaseJwtKeyFactory.CreateSigningKey(supabaseJwtSecret);
        }

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");
                logger.LogWarning(context.Exception, "Fallo al validar JWT de Supabase.");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new AdminRequirement()));
});
builder.Services.AddScoped<IAuthorizationHandler, AdminRequirementHandler>();

var corsOrigins = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>()?
    .AllowedOrigins ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("UrbanfixWeb", policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 25 * 1024 * 1024;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.ApplyDatabaseMigrationsAsync();
}

app.UseHttpsRedirection();
app.UseCors("UrbanfixWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
