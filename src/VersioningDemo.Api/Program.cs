using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using VersioningDemo.Api.Swagger;
using VersioningDemo.Application;
using VersioningDemo.Infrastructure;
using VersioningDemo.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Needed so versioning errors (e.g. unsupported version) come back as ProblemDetails.
builder.Services.AddProblemDetails();

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

public partial class Program;
