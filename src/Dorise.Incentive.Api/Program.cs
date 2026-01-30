using Dorise.Incentive.Application;
using Dorise.Incentive.Api.Documentation;
using Dorise.Incentive.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Starting Dorise Incentive API - 'Me fail English? That's unpossible!'");
app.Run();

/// <summary>
/// Program class for integration testing.
/// </summary>
public partial class Program { }
