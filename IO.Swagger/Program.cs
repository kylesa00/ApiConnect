using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using IO.Swagger.Filters;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
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
            Description = "Webshop Service API (ASP.NET Core 3.1)",
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

// Configure IIS Server options (only needed when hosting in IIS)
builder.Services.Configure<IISServerOptions>(options => 
{ 
    options.AllowSynchronousIO = true; 
});

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
