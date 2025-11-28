using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using PatientService.API.Data;
using PatientService.API.Data.Repositories;
using PatientService.API.Domain.Repositories;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore; // added for Database.MigrateAsync

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<PatientDbContext>(connectionName: "PatientApp");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();
builder.AddServiceDefaults();



var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

ArgumentException.ThrowIfNullOrWhiteSpace(jwtKey, "JWT Key is not configured.");


builder.Services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            // Ensure role claims from JWT are recognized as roles
            RoleClaimType = "role"
        };
    });

// Require authenticated users by default for endpoints and add a policy for patients
builder.Services.AddAuthorization(options => {
    // options.FallbackPolicy = new AuthorizationPolicyBuilder()
    //     .RequireAuthenticatedUser()
    //     .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    //     .Build();

    // Policy that requires the user to have the "Patient" role
    options.AddPolicy("IsPatient", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole("Patient")
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    );
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IPatientRepository, PatientNpgRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    // Apply EF Core migrations automatically in Development environment.
    // This ensures the local dev database is brought up-to-date when the app starts.
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    // try
    // {
    //     var db = services.GetRequiredService<PatientDbContext>();
    //     await db.Database.MigrateAsync();
    //     logger.LogInformation("Database migrations applied successfully.");
    // }
    // catch (Exception ex)
    // {
    //     logger.LogError(ex, "An error occurred while migrating the database.");
    //     throw;
    // }
}

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();

app.Run();