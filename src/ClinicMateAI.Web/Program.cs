using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Ai;
using ClinicMateAI.Application.Inbox;
using ClinicMateAI.Application.Messaging;
using ClinicMateAI.Application.Promotions;
using ClinicMateAI.Application.Clinics;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Infrastructure.Persistence;
using ClinicMateAI.Logic.Ai;
using ClinicMateAI.Logic.Inbox;
using ClinicMateAI.Logic.Messaging;
using ClinicMateAI.Logic.Promotions;
using ClinicMateAI.Logic.Clinics;
using ClinicMateAI.Logic.Setup;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Web.Services;
using ClinicMateAI.Web.Endpoints;
using ClinicMateAI.Web.Components;
using ClinicMateAI.Web.Components.Account;
using ClinicMateAI.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ToastService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddValidatorsFromAssemblyContaining<ReceiveMessageCommandValidator>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IClinicServiceRepository, ClinicServiceRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IReceiveMessageHandler, ReceiveMessageHandler>();
builder.Services.AddScoped<IGetInboxClinicsHandler, GetInboxClinicsHandler>();
builder.Services.AddScoped<IGetInboxConversationsHandler, GetInboxConversationsHandler>();
builder.Services.AddScoped<IGetConversationMessagesHandler, GetConversationMessagesHandler>();
builder.Services.AddScoped<IGetAvailablePromotionsHandler, GetAvailablePromotionsHandler>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IGetSetupOverviewHandler, GetSetupOverviewHandler>();
builder.Services.AddScoped<IUpsertClinicProfileHandler, UpsertClinicProfileHandler>();
builder.Services.AddScoped<IAddClinicServiceHandler, AddClinicServiceHandler>();
builder.Services.AddScoped<IGetClinicServicesHandler, GetClinicServicesHandler>();
builder.Services.AddScoped<IDeleteClinicServiceHandler, DeleteClinicServiceHandler>();
builder.Services.AddScoped<IGetClinicsHandler, GetClinicsHandler>();
builder.Services.AddScoped<ICreateClinicHandler, CreateClinicHandler>();
builder.Services.AddScoped<IAiReplyProvider, SimulatedAiReplyProvider>();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var identityDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (identityDb.Database.IsRelational())
    {
        await identityDb.Database.MigrateAsync();
    }
    else
    {
        await identityDb.Database.EnsureCreatedAsync();
    }

    if (appDb.Database.IsRelational())
    {
        await appDb.Database.MigrateAsync();
    }
    else
    {
        await appDb.Database.EnsureCreatedAsync();
    }

    await DemoDataSeeder.SeedAsync(appDb);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapWebhookEndpoints();
app.MapInboxEndpoints();
app.MapPromotionsEndpoints();
app.MapSetupEndpoints();
app.MapClinicsEndpoints();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();

public partial class Program;
