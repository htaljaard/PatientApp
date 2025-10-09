var builder = DistributedApplication.CreateBuilder(args);

var dbServer = builder.AddSqlServer("sql")
                      .WithHostPort(1433)
                      .WithLifetime(ContainerLifetime.Persistent);

var db = dbServer.AddDatabase("PatientApp");

var userService = builder.AddProject<Projects.UserService_API>("user-service")
                         .WithReference(db)
                         .WaitFor(db);

builder.Build().Run();
