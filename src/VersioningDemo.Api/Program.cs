using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using VersioningDemo.Api.Handlers;
using VersioningDemo.Api.Swagger;
using VersioningDemo.Application;
using VersioningDemo.Infrastructure;
using VersioningDemo.Infrastructure.Persistence;

// Simple startup logger (not a "bootstrap/reloadable" one: that one can only be frozen once per process,
// which breaks multiple WebApplicationFactory instances in the integration tests).
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting VersioningDemo API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddControllers();

    // Needed so versioning errors (e.g. unsupported version) come back as ProblemDetails.
    builder.Services.AddProblemDetails();

    // Logs every unhandled exception exactly once and returns a ProblemDetails 500.
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddApplication();
  builder.Services.AddInfrastructure(builder.Configuration);
  

    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = false; // never silently serve another version
            options.ReportApiVersions = true;                    // api-supported-versions / api-deprecated-versions headers
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    // Order matters: exception handler first, then request logging, so failed requests are logged with their 500 status.
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var d in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{d.GroupName}/swagger.json",
                d.IsDeprecated ? $"{d.GroupName.ToUpperInvariant()} (deprecated)" : d.GroupName.ToUpperInvariant());
        }
    });

    app.MapControllers();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
