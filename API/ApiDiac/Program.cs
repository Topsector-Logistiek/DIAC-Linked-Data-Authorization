using ApiDiac.Data;
using ApiDiac.Data.Interfaces;
using ApiDiac.Services;
using ApiDiac.Services.Interfaces;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Poort8.Ishare.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(o => o.SerializerSettings.Converters.Add(new StringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DIAC API",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Adoptie Support Team - Topsector Logistiek",
            Url = new Uri("https://topsectorlogistiek.atlassian.net/servicedesk/customer/portal/2"),
        },
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token, this should be a JWT token obtained from the ISHARE/token endpoint",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});
builder.Services.AddSwaggerGenNewtonsoftSupport();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddIshareCoreServices();

builder.Services.AddScoped<IDataHandler, SparqlDataHandler>();
builder.Services.AddScoped<IQueryService, QueryService>();
builder.Services.AddScoped<IIshareAuthService, IshareAuthService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DIAC API V1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();