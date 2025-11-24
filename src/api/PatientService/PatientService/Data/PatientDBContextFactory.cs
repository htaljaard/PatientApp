// ...existing code...
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace PatientService.API.Data;

// Design-time factory used by EF Core tools to create PatientDBContext.
internal sealed class PatientDBContextFactory : IDesignTimeDbContextFactory<PatientDbContext>
{
    public PatientDbContext CreateDbContext(string[] args)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Try common locations for the connection string. The application registers the DbContext
        // with the connection name "PatientApp" (see Program.cs). Respect that here.
        var connectionString = config.GetConnectionString("PatientApp")
            ?? config["ConnectionStrings:PatientApp"]
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__PatientApp")
            // Fallback to a sensible local default for developer machines.
            ?? "Host=localhost;Database=patientapp;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<PatientDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(PatientDbContext).Assembly.FullName));

        return new PatientDbContext(optionsBuilder.Options);
    }
}
// ...existing code...

