using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using IO.Swagger.Filters;
using IO.Swagger.Helpers;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Use Serilog with the already built configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Add framework services.
builder.Services
    .AddControllers(options =>
    {
        options.InputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>();
        options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>();
    })
    .AddNewtonsoftJson(opts =>
    {
        opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
    })
    .AddXmlSerializerFormatters();

builder.Services
    .AddSwaggerGen(c =>
    {
        c.SwaggerDoc("2.0-draftAv", new OpenApiInfo
        {
            Version = "2.0-draftAv",
            Title = "Webshop Service API",
            Description = "Webshop Service API (ASP.NET 10)",
            Contact = new OpenApiContact()
            {
                Name = "Swagger Codegen Contributors",
                Url = new Uri("https://github.com/swagger-api/swagger-codegen"),
                Email = ""
            },
        });
        c.CustomSchemaIds(type => type.FullName);
        var env = builder.Environment;
        var xmlFile = $"{env.ApplicationName}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
        c.DocumentFilter<BasePathFilter>("/apps/prod-webshop-service-app/webshop-service");
        c.OperationFilter<GeneratePathParamsValidationFilter>();
    });

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// In-memory cache used by DatabaseCacheService and order validation
builder.Services.AddMemoryCache();

// Background service that loads reference data from the DB into the cache at startup.
// Registered as a singleton first so it can also be injected directly into controllers.
builder.Services.AddSingleton<IO.Swagger.Helpers.DatabaseCacheService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<IO.Swagger.Helpers.DatabaseCacheService>());

// Register Dal as a scoped service for DI
builder.Services.AddScoped<Dal>();

// Register NavWebServiceReferenceOptions for DI
builder.Services.Configure<NavWebServiceReferenceOptions>(
    builder.Configuration.GetSection("WebServiceReference"));

// Configure IIS Server options (only needed when hosting in IIS)
//builder.Services.Configure<IISServerOptions>(options => 
//{ 
//    options.AllowSynchronousIO = true; 
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Order is important: Exception handling should be first

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Swagger configuration
var swaggerBasePath = "swagger";
app.UseSwagger(c =>
{
    c.RouteTemplate = swaggerBasePath + "/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"/{swaggerBasePath}/2.0-draftAv/swagger.json", "Webshop Service API");
    c.RoutePrefix = swaggerBasePath;
});

// Static files should come before routing
app.UseStaticFiles();

// Routing middleware
app.UseRouting();

// Authentication/Authorization (if needed)
//app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();


