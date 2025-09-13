using CustomerSupportSystem.Web.Components;
using CustomerSupportSystem.Data.Context;
using CustomerSupportSystem.Data.Seed;
using CustomerSupportSystem.Domain.Entities;
using CustomerSupportSystem.Web.Authorization;
using CustomerSupportSystem.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add SignalR
builder.Services.AddSignalR();

// Add authentication and authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    // Ticket transition policy
    options.AddPolicy(Policies.CanTransitionTicket, policy =>
        policy.Requirements.Add(new TicketTransitionRequirement()));

    // Own ticket edit policy (24 hour time limit)
    options.AddPolicy(Policies.CanEditOwnTicket, policy =>
        policy.Requirements.Add(new OwnTicketEditRequirement(TimeSpan.FromHours(24))));

    // Role-based policies
    options.AddPolicy(Policies.CanManageUsers, policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy(Policies.CanViewAllTickets, policy => policy.RequireRole(Roles.Admin, Roles.Agent));
    options.AddPolicy(Policies.CanManageAttachments, policy => policy.RequireRole(Roles.Admin, Roles.Agent));
    options.AddPolicy(Policies.CanManageEmailIngestion, policy => policy.RequireRole(Roles.Admin, Roles.Agent));
});

// Add authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, TicketTransitionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, OwnTicketEditHandler>();

// Add custom services
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IEmailIngestionService, EmailIngestionService>();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure path base for Elastic Beanstalk
if (!app.Environment.IsDevelopment())
{
    app.UsePathBase("/");
}

// Map API controllers
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    await DataSeeder.SeedAsync(context, userManager, roleManager);
}

app.Run();
