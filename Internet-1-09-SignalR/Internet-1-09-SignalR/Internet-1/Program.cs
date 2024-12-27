using AspNetCoreHero.ToastNotification;
using AutoMapper;
using Internet_1.Hubs;
using Internet_1.Localisation;
using Internet_1.Models;
using Internet_1.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<LessonRepository>();
builder.Services.AddScoped<VideoRepository>();
builder.Services.AddScoped<InstructorRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<TodoRepository>();
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("sqlCon"));
});
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(2);
});
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireUppercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
    options.Lockout.MaxFailedAccessAttempts = 3;
})
.AddDefaultTokenProviders()
.AddErrorDescriber<ErrorDescription>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 10;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
});
builder.Services.ConfigureApplicationCookie(opt =>
{
    var cookiBuilder = new CookieBuilder
    {
        Name = "IdentyMvcCookie"
    };
    opt.LoginPath = new PathString("/Home/Login");
    opt.LogoutPath = new PathString("/Home/Logout");
    opt.AccessDeniedPath = new PathString("/Home/AccessDenied");
    opt.Cookie = cookiBuilder;
    opt.ExpireTimeSpan = TimeSpan.FromDays(15);
    opt.SlidingExpiration = true;
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 25 * 1024 * 1024; // 25 MB limit for file uploads
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2); // Increase timeout
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2); // Increase header timeout
});

builder.Services.AddControllers(
    options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400000;
});

var app = builder.Build();

// Admin rolü ve kullanıcısı oluştur
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    // Rolleri oluştur
    var roles = new[] { "Admin", "Uye", "Eğitmen" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new AppRole { Name = role });
        }
    }

    // Admin kullanıcısını oluştur
    var adminUser = await userManager.FindByNameAsync("admin");
    if (adminUser == null)
    {
        var admin = new AppUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            FullName = "Admin User",
            PhotoUrl = "no-image.jpg"
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

// Configure the HTTP request pipeline.
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

// SignalR endpoint'lerini UseEndpoints içinde tanımla
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapHub<GeneralHub>("/generalhub");
});

app.Run();
