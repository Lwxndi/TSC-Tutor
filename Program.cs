//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using System.Diagnostics;
//using Tutor_Manager.Models;
//using Tutor_Manager.Services;
//using Tutor_Manager.Services.Academic;
//using Tutor_Manager.Services.Activation;
//using Tutor_Manager.Services.Email;
//using Tutor_Manager.Services.Notifications;
//using Tutor_Manager.Services.Upload;
//using Tutor_Manager.Helpers;
//using Tutor_Manager.Options;
//using Tutor_Manager.Services.QuizzServices;
//using Tutor_Manager.Services.AssessmentServices;


//var builder = WebApplication.CreateBuilder(args);
//if (builder.Environment.IsDevelopment())
//{
//    builder.Configuration.AddUserSecrets("52f40f7a-bde5-441b-b558-c266b130dfd6");
//}
//Console.WriteLine($"[DEBUG] Gemini:ApiKey from Configuration = '{builder.Configuration["Gemini:ApiKey"]}'");
//var connectionString = builder.Configuration.GetConnectionString("Tutor_ManagerDatabaseContext") ?? throw new InvalidOperationException("Connection string 'Tutor_ManagerDatabaseContext' not found.");

//builder.Services.AddDbContext<Tutor_ManagerDatabaseContext>(options => options.UseSqlServer(connectionString));
//builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
//builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
//builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
//builder.Services.AddScoped<IEmailService, SmtpEmailService>();
//builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
//builder.Services.AddScoped<INotificationService, NotificationService>();
//builder.Services.AddScoped<IApplicationFileStorageService, ApplicationFileStorageService>();
//builder.Services.AddScoped<IAccountActivationService, AccountActivationService>();
//builder.Services.AddScoped<IAcademicService, AcademicService>();
//builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
//builder.Services.AddScoped<ITutorManagementService, TutorManagementService>();
//builder.Services.AddScoped<ISessionService, SessionService>();
//builder.Services.AddScoped<IOfferingService, OfferingService>();
//builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
//builder.Services.AddHttpClient("Gemini");
//builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();
//builder.Services.AddScoped<ITutorAssignmentService, TutorAssignmentService>();
//builder.Services.AddScoped<IStudyMaterialFileStorageService, StudyMaterialFileStorageService>();
//builder.Services.AddScoped<IStudyMaterialService, StudyMaterialService>();
//builder.Services.AddScoped<Tutor_Manager.Services.DataSeeding.TutorSeedService>();
//builder.Services.AddScoped<IQuizService, QuizService>();
//builder.Services.AddScoped<Tutor_Manager.Services.QuizzServices.IStudyMaterialTextExtractionService,
//    Tutor_Manager.Services.QuizzServices.StudyMaterialTextExtractionService>();
//builder.Services.AddScoped<IQuizGenerationService, QuizGenerationService>();
////builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
//builder.Services.AddScoped<IQuizQuestionService, QuizQuestionService>();
//builder.Services.AddScoped<IQuizAttemptMarkingService, QuizAttemptMarkingService>();
//builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
//builder.Services.AddScoped<IAssessmentFileStorageService, AssessmentFileStorageService>();
//builder.Services.AddScoped<IAssessmentService, AssessmentService>();
//builder.Services.AddScoped<IAssessmentGenerationService, AssessmentGenerationService>();
//builder.Services.AddSession();
//// Add services to the container.
//builder.Services.AddControllersWithViews();

//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Home/Login";
//        options.AccessDeniedPath = "/Home/Login";
//    });


//var app = builder.Build();
//using (var scope = app.Services.CreateScope())
//{
//    var seeder = scope.ServiceProvider.GetRequiredService<Tutor_Manager.Services.DataSeeding.TutorSeedService>();
//    await seeder.SeedAsync();
//}

//var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Tutor_Manager.Models.User>();
//var hash = hasher.HashPassword(null!, "Password@01Pass");
//Console.WriteLine(hash);

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseRouting();
//app.UseAuthentication();

//// Enable Session
//app.UseSession();

//app.UseAuthorization();

//app.MapStaticAssets();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}")
//    .WithStaticAssets();


//app.Run();

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Tutor_Manager.Helpers;
using Tutor_Manager.Infrastructure;
using Tutor_Manager.Models;
using Tutor_Manager.Options;
using Tutor_Manager.Services;
using Tutor_Manager.Services.Academic;
using Tutor_Manager.Services.Activation;
using Tutor_Manager.Services.AiCompletion;
using Tutor_Manager.Services.AiProviders;
using Tutor_Manager.Services.AssessmentServices;
using Tutor_Manager.Services.Email;
using Tutor_Manager.Services.EnrollmentServices;
using Tutor_Manager.Services.Notifications;
using Tutor_Manager.Services.QuizzServices;
using Tutor_Manager.Services.StudyMaterialTextExtraction;
using Tutor_Manager.Services.Upload;
using System.Text.Json.Serialization;



var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets("52f40f7a-bde5-441b-b558-c266b130dfd6");
}
Console.WriteLine($"[DEBUG] Gemini:ApiKey from Configuration = '{builder.Configuration["Gemini:ApiKey"]}'");
var connectionString = builder.Configuration.GetConnectionString("Tutor_ManagerDatabaseContext") ?? throw new InvalidOperationException("Connection string 'Tutor_ManagerDatabaseContext' not found.");

builder.Services.AddDbContext<Tutor_ManagerDatabaseContext>(options => options.UseSqlServer(connectionString).EnableSensitiveDataLogging());
builder.Services.AddScoped<IEmailService, ConsoleEmailService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IApplicationFileStorageService, ApplicationFileStorageService>();
builder.Services.AddScoped<IAccountActivationService, AccountActivationService>();
builder.Services.AddScoped<IAcademicService, AcademicService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITutorManagementService, TutorManagementService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IOfferingService, OfferingService>();
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection("Gemini"));
builder.Services.AddHttpClient("Gemini");
builder.Services.Configure<GroqOptions>(builder.Configuration.GetSection("Groq"));
builder.Services.AddHttpClient("Groq");
builder.Services.AddScoped<GeminiTextCompletionProvider>();
builder.Services.AddScoped<GroqTextCompletionProvider>();
builder.Services.AddScoped<IAiCompletionOrchestrator, AiCompletionOrchestrator>();
builder.Services.AddScoped<IAiRecommendationService, AiRecommendationService>();
builder.Services.AddScoped<IAiCompletionOrchestrator, AiCompletionOrchestrator>();
builder.Services.AddScoped<ITutorAssignmentService, TutorAssignmentService>();
builder.Services.AddScoped<IStudyMaterialFileStorageService, StudyMaterialFileStorageService>();
builder.Services.AddScoped<IStudyMaterialService, StudyMaterialService>();
builder.Services.AddScoped<Tutor_Manager.Services.DataSeeding.TutorSeedService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IStudyMaterialTextExtractionService, StudyMaterialTextExtractionService>();
builder.Services.AddScoped<IQuizGenerationService, QuizGenerationService>();
builder.Services.AddSingleton<IQuizImageStorage, QuizImageStorage>();
builder.Services.AddScoped<IStudyMaterialImageExtractionService, StudyMaterialImageExtractionService>();
//builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IQuizQuestionService, QuizQuestionService>();
builder.Services.AddScoped<IQuizAttemptMarkingService, QuizAttemptMarkingService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddScoped<IAssessmentFileStorageService, AssessmentFileStorageService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IAssessmentGenerationService, AssessmentGenerationService>();
builder.Services.AddScoped<Tutor_Manager.Services.PaymentServices.IPaymentService,
                            Tutor_Manager.Services.PaymentServices.PaymentService>();
builder.Services.Configure<Tutor_Manager.Options.StripeOptions>(
    builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<GroqTextCompletionProvider>(); 

// Was missing — AssessmentController depends on this.
builder.Services.AddScoped<IAssessmentQuestionService, AssessmentQuestionService>();
builder.Services.AddScoped<IAssessmentAttemptMarkingService, AssessmentAttemptMarkingService>();
builder.Services.AddScoped<IAssessmentAttemptService, AssessmentAttemptService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

builder.Services.Configure<Tutor_Manager.Options.BusinessOptions>(
      builder.Configuration.GetSection("Business"));
builder.Services.AddScoped<Tutor_Manager.Services.ReceiptServices.IReceiptService,
                           Tutor_Manager.Services.ReceiptServices.ReceiptService>();


builder.Services.AddSession();
// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opts.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter());
    opts.JsonSerializerOptions.Converters.Add(new NullableTimeSpanJsonConverter());
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/Login";
    });


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<Tutor_Manager.Services.DataSeeding.TutorSeedService>();
    await seeder.SeedAsync();
}
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
Console.WriteLine($"[DEBUG] Stripe:SecretKey length = {Stripe.StripeConfiguration.ApiKey?.Length ?? 0}");

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Tutor_Manager.Models.User>();
var hash = hasher.HashPassword(null!, "Password@01Pass");
Console.WriteLine(hash);

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