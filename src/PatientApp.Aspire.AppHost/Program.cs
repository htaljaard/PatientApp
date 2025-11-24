using PatientApp.Aspire.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var databaseName = "patientappdb";
var password = builder.AddParameter("password", secret: true);
var creationScript = $$"""
                       -- Create the database
                       CREATE DATABASE {{databaseName}};

                       """;

#pragma warning disable ASPIREPROXYENDPOINTS001
var dbServer = builder
    .AddPostgres("postgres")
    .WithHostPort(5432)
    .WithPgAdmin()
    .WithPgWeb()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpointProxySupport(false)
#pragma warning restore ASPIREPROXYENDPOINTS001
    .WithDataVolume("patientapp-data")
    .WithPassword(password);
var db = dbServer.AddDatabase(databaseName).WithCreationScript(creationScript);

var userService = builder.AddProject<Projects.UserService_API>("user-service")
    .WithReference(db)
    .WaitFor(db)
    .AddScalar()
    .AddJWT();

var patientService = builder.AddProject<Projects.PatientService_API>("patient-service")
    .WithReference(db)
    .WaitFor(db)
    .AddScalar()
    .AddJWT();


builder.Build().Run();