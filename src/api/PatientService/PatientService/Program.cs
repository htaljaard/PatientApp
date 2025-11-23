using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using PatientService.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddNpgsqlDbContext<PatientDBContext>(connectionName: "PatientApp");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.AddServiceDefaults();

builder.Services.AddFastEndpoints();

var jwtKey = builder.Configuration["Jwt__Key"];
var jwtIssuer = builder.Configuration["Jwt__Issuer"];
var jwtAudience = builder.Configuration["Jwt__Audience"];

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
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .Build();

    // Policy that requires the user to have the "Patient" role
    options.AddPolicy("IsPatient", policy =>
        policy.RequireAuthenticatedUser()
            .RequireRole("Patient")
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
    );
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();

app.Run();