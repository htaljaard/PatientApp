using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatientApp.SharedKernel.Auth;
using Scalar.AspNetCore;
using UserService.API;
using UserService.API.Data;
using UserService.API.Domain;
using UserService.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));


builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints();

builder.Services.AddAuthorization();


builder.AddServiceDefaults(); //Aspire

builder.AddNpgsqlDbContext<AppDbContext>(connectionName:"PatientApp");

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, FakeEmailSender>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoleManager<RoleManager<IdentityRole>>()
    .AddUserManager<UserManager<ApplicationUser>>()
    .AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

//app.MapIdentityApi<ApplicationUser>();
app.UseFastEndpoints();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();

app.Run();