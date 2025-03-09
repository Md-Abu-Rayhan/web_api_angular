using Application.Services;
using Domain.Interfaces;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using Domain.Settings;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth API",
        Version = "v1",
        Description = "Authentication API with JWT support"
    });

    // Security definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Security requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// Authentication configuration
// Authentication configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
        // Add these to resolve common issues
        ClockSkew = TimeSpan.Zero,
        NameClaimType = "email",  // Match your claim names
        RoleClaimType = "role"
    };
});

// Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngularDev",
//        policy => policy
//            .WithOrigins(
//                "http://localhost:4200",  // Default Angular port
//                "http://localhost:10565"  // Your specific port
//            )
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials());
//});


builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAngularDev",
            policy => policy
                .SetIsOriginAllowed(origin =>
                    new Uri(origin).Host == "localhost" ||
                    new Uri(origin).Host == "127.0.0.1"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    }
    else
    {
        var allowedOrigins = builder.Configuration["AllowedOrigins"]
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        options.AddPolicy("AllowAngularDev",
            policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    }
});



builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<IProductService, ProductService>();

// Service registrations
builder.Services.AddInfrastructure();  // Registers repositories
builder.Services.AddScoped<IProductService, ProductService>();  // Registers service


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddOptions<JwtSettings>()
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

// Configure the HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1");
    });
}

//app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAngularDev"); 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseWebSockets();

app.Run();