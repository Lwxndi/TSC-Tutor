using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tutor_Manager.Models;
using Tutor_Manager.Services.Activation;
using Tutor_Manager.Services.Email;
using Tutor_Manager.Services.Notifications;
using System.Diagnostics;
using Tutor_Manager.Services.Upload;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Tutor_ManagerDatabaseContext") ?? throw new InvalidOperationException("Connection string 'Tutor_ManagerDatabaseContext' not found.");

builder.Services.AddDbContext<Tutor_ManagerDatabaseContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IApplicationFileStorageService, ApplicationFileStorageService>();
builder.Services.AddScoped<IAccountActivationService, AccountActivationService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/Login";
    });


var app = builder.Build();

//var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Tutor_Manager.Models.User>();
//var hash = hasher.HashPassword(null!, "Password@01Pass");
//Console.WriteLine(hash);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();

// Enable Session
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();