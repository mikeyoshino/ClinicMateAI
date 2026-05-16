using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Abstractions.Auth;
using ClinicMateAI.Application.Ai;
using ClinicMateAI.Application.Inbox;
using ClinicMateAI.Application.Messaging;
using ClinicMateAI.Application.Promotions;
using ClinicMateAI.Application.Clinics;
using ClinicMateAI.Application.Branches;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Infrastructure.Messaging;
using ClinicMateAI.Infrastructure.Persistence;
using ClinicMateAI.Logic.Ai;
using ClinicMateAI.Logic.Inbox;
using ClinicMateAI.Logic.Messaging;
using ClinicMateAI.Logic.Promotions;
using ClinicMateAI.Logic.Clinics;
using ClinicMateAI.Logic.Branches;
using ClinicMateAI.Logic.Setup;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Web.Services;
using ClinicMateAI.Web.Endpoints;
using ClinicMateAI.Web.Hubs;
using ClinicMateAI.Web.Components;
using ClinicMateAI.Web.Middleware;
using ClinicMateAI.Web.Components.Account;
using ClinicMateAI.Web.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient();
builder.Services.AddSignalR();
builder.Services.AddExceptionHandler<BusinessExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<IInboxNotifier, SignalRInboxNotifier>();

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
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IClinicUserProfileRepository, ClinicUserProfileRepository>();
builder.Services.AddScoped<IUserBranchAssignmentRepository, UserBranchAssignmentRepository>();
builder.Services.AddScoped<IBranchAccessPolicy, BranchAccessPolicy>();
builder.Services.AddScoped<IClinicServiceRepository, ClinicServiceRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IReceiveMessageHandler, ReceiveMessageHandler>();
builder.Services.AddScoped<IGetInboxClinicsHandler, GetInboxClinicsHandler>();
builder.Services.AddScoped<IGetInboxConversationsHandler, GetInboxConversationsHandler>();
builder.Services.AddScoped<IGetConversationMessagesHandler, GetConversationMessagesHandler>();
builder.Services.AddScoped<IMarkConversationReadHandler, MarkConversationReadHandler>();
builder.Services.AddScoped<IClaimConversationHandler, ClaimConversationHandler>();
builder.Services.AddScoped<IReleaseConversationHandler, ReleaseConversationHandler>();
builder.Services.AddScoped<IGetAvailablePromotionsHandler, GetAvailablePromotionsHandler>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IGetSetupOverviewHandler, GetSetupOverviewHandler>();
builder.Services.AddScoped<IGetIntegrationOverviewHandler, GetIntegrationOverviewHandler>();
builder.Services.AddScoped<ISaveLineChannelConfigHandler, SaveLineChannelConfigHandler>();
builder.Services.AddScoped<ITestLineChannelConfigHandler, TestLineChannelConfigHandler>();
builder.Services.AddScoped<IStartFacebookConnectionHandler, StartFacebookConnectionHandler>();
builder.Services.AddScoped<ICompleteFacebookConnectionHandler, CompleteFacebookConnectionHandler>();
builder.Services.AddScoped<IRenewFacebookConnectionHandler, RenewFacebookConnectionHandler>();
builder.Services.AddScoped<IUpsertClinicProfileHandler, UpsertClinicProfileHandler>();
builder.Services.AddScoped<IAddClinicServiceHandler, AddClinicServiceHandler>();
builder.Services.AddScoped<IGetClinicServicesHandler, GetClinicServicesHandler>();
builder.Services.AddScoped<IDeleteClinicServiceHandler, DeleteClinicServiceHandler>();
builder.Services.AddScoped<IGetClinicsHandler, GetClinicsHandler>();
builder.Services.AddScoped<ICreateClinicHandler, CreateClinicHandler>();
builder.Services.AddScoped<ICreateBranchHandler, CreateBranchHandler>();
builder.Services.AddScoped<IUpdateBranchHandler, UpdateBranchHandler>();
builder.Services.AddScoped<IDeactivateBranchHandler, DeactivateBranchHandler>();
builder.Services.AddScoped<IAssignUserToBranchHandler, AssignUserToBranchHandler>();
builder.Services.AddScoped<IRemoveUserFromBranchHandler, RemoveUserFromBranchHandler>();
builder.Services.AddScoped<IGetBranchesHandler, GetBranchesHandler>();
builder.Services.AddScoped<IGetAccessibleBranchesHandler, GetAccessibleBranchesHandler>();
builder.Services.AddScoped<IAiReplyProvider, SimulatedAiReplyProvider>();

// LINE webhook infrastructure
builder.Services.AddScoped<IClinicChannelConfigRepository, ClinicChannelConfigRepository>();
builder.Services.AddSingleton<ILineSignatureVerifier, LineSignatureVerifier>();
builder.Services.AddSingleton<ILineWebhookParser, LineWebhookParser>();
builder.Services.AddHttpClient<ILineMessageSender, LineMessageSender>();
builder.Services.AddHttpClient<ILineProfileProvider, LineProfileProvider>();
builder.Services.AddHttpClient<ILineChannelConnectionTester, LineChannelConnectionTester>();
builder.Services.AddScoped<IFacebookConnectionProvider, FacebookConnectionProvider>();
builder.Services.AddHttpClient<IFacebookTokenRenewalProvider, FacebookTokenRenewalProvider>();

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
        await identityDb.Database.MigrateAsync();
    else
        await identityDb.Database.EnsureCreatedAsync();

    if (appDb.Database.IsRelational())
        await appDb.Database.MigrateAsync();
    else
        await appDb.Database.EnsureCreatedAsync();

    await DemoDataSeeder.SeedAsync(appDb);
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// BusinessExceptionHandler runs in all environments so API callers get structured errors
app.UseExceptionHandler();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<InboxHub>("/hubs/inbox");

app.MapWebhookEndpoints();
app.MapInboxEndpoints();
app.MapPromotionsEndpoints();
app.MapSetupEndpoints();
app.MapIntegrationEndpoints();
app.MapClinicsEndpoints();

app.MapAdditionalIdentityEndpoints();

app.Run();

public partial class Program;
