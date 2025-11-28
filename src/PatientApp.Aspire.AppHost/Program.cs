using PatientApp.Aspire.AppHost.Extensions;

#pragma warning disable ASPIREPROXYENDPOINTS001
var builder = DistributedApplication.CreateBuilder(args);

var userName = builder.AddParameter("username");
var password = builder.AddParameter("password", secret: true);
var rabbitMqPassword = builder.AddParameter("rabbitMQ-password", secret: true);

var databaseName = "patientappdb";
var creationScript = $$"""
                       -- Create the database
                       CREATE DATABASE {{databaseName}};

                       """;
var dbServer = builder
    .AddPostgres("postgres")
    .WithHostPort(5432)
    .WithPgAdmin()
    .WithPgWeb()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEndpointProxySupport(false)
    .WithDataVolume("patientapp-data")
    .WithPassword(password);
var db = dbServer.AddDatabase(databaseName).WithCreationScript(creationScript);

var rabbitMq = builder
    .AddRabbitMQ("rabbitMQ",userName,rabbitMqPassword,53957)
    .WithManagementPlugin()
    .WaitFor(rabbitMqPassword)
    .WithLifetime(ContainerLifetime.Persistent);

var userService = builder.AddProject<Projects.UserService_API>("user-service")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(rabbitMq)
    .AddScalar()
    .AddJWT();

var patientService = builder.AddProject<Projects.PatientService_API>("patient-service")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(rabbitMq)
    .AddScalar()
    .AddJWT();


builder.Build().Run();


#pragma warning restore ASPIREPROXYENDPOINTS001