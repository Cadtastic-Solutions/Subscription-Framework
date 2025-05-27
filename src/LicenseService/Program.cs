using LicenseService.Interfaces;
using LicenseService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System;
using System.IO;
using System.Reflection;

// Early init of NLog to allow startup and exception logging
var logger = LogManager.Setup()
                        .LoadConfigurationFromAppSettings()
                        .GetCurrentClassLogger();

try
{
    logger.Info("Initializing License Service");

    var builder = WebApplication.CreateBuilder(args);

    // Add NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container
    builder.Services.AddControllers();

    // Add Swagger with XML comments
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { 
            Title = "License Service API", 
            Version = "v1",
            Description = "API for license validation and Stripe subscription integration",
            Contact = new OpenApiContact
            {
                Name = "License Service Team"
            }
        });

        // Set the comments path for the Swagger JSON and UI
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
        
        // Add the APIM subscription key as a security definition
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = "Ocp-Apim-Subscription-Key",
            Description = "License key provided by APIM"
        });
        
        // Add security requirement for all operations
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                },
                Array.Empty<string>()
            }
        });
    });

    // Register our services
    builder.Services.AddSingleton<ILicenseRepository, InMemoryLicenseRepository>();
    builder.Services.AddScoped<ICryptoService, CryptoService>();
    builder.Services.AddScoped<ILicenseService, LicenseServiceImpl>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/", () => "License Service API");

    app.Run();
}
catch (Exception ex)
{
    // NLog: catch setup errors
    logger.Error(ex, "License Service stopped due to exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}
