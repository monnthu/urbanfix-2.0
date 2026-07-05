using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Urbanfix.Models;
using Urbanfix.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MapSettings>(builder.Configuration.GetSection("MapSettings"));
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

var connectionString = builder.Configuration.GetConnectionString("UrbanfixDb");
if (!string.IsNullOrWhiteSpace(connectionString)
    && !connectionString.Contains("TU_SERVIDOR", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<Urbanfix.Data.UrbanfixDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddScoped<PuntoService>();
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ReportApiService>();
builder.Services.AddHttpClient<SupabaseAuthService>();
builder.Services.AddSingleton<InstitutionDirectoryService>();
builder.Services.AddScoped<InstitutionRoutingService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(ProfileRoles.Admin));
    options.AddPolicy("InstitutionOnly", policy => policy.RequireRole(ProfileRoles.Institution));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
