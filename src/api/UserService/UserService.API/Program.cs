using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;

using UserService.API.Data;
using UserService.API.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.AddSqlServerDbContext<AppDbContext>("PatientApp");


builder.Services.AddOpenApi();

builder.AddServiceDefaults(); //Aspire

// builder.Services.AddDbContext<AppDbContext>(o =>
//                     o.UseSqlServer(builder.Configuration.GetConnectionString("UserServiceDb")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ApplicationUser>();

app.UseHttpsRedirection();
app.MapDefaultEndpoints();

app.Run();

