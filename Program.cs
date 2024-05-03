using Job.Services;
using Job.Abstraction.Repositories;
using Job.Abstraction.Services;
using Job.DataRepository.Repositories;
using Job.Services.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
{
    var services = builder.Services;
    services.AddHttpContextAccessor();
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.AccessDeniedPath = "/Account/Login";
        });

    services.AddHttpClient();

    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    services.AddScoped<ClaimsPrincipal>();

    services.AddScoped<IAccountServices, AccountServices>();
    services.AddScoped<IAccountRepository, AccountRepository>();
    services.AddScoped<IdropdownMasterService, DropdownMasterService>();
    services.AddScoped<IDropdownMasterRepository, DropdownMasterRepository>();
    services.AddScoped<IJobApplicationServices, JobApplicationServices>();
    services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
    services.AddScoped<ClaimsPrincipal>();
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
