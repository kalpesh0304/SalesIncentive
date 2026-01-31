using Azure.Identity;
using Dorise.Incentive.Application;
using Dorise.Incentive.Api.Documentation;
using Dorise.Incentive.Infrastructure;
using Dorise.Incentive.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault configuration provider
var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add API versioning and documentation
builder.Services.AddApiVersioningServices();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddHealthChecks();

// Add Authentication with Azure AD / JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add Authorization with custom permission-based policy provider
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, ExtendedPermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AnyPermissionAuthorizationHandler>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Starting Dorise Incentive API - 'Me fail English? That's unpossible!'");
app.Run();

/// <summary>
/// Program class for integration testing.
/// </summary>
public partial class Program { }
