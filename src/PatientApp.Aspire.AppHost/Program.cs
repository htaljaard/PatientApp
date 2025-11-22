using Aspire.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

var builder = DistributedApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection("Jwt");

var password = builder.AddParameter("password", secret: true);

#pragma warning disable ASPIREPROXYENDPOINTS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var dbServer = builder.AddSqlServer("sql")
                      .WithPassword(password)
                      .WithHostPort(1433)
                      .WithDataVolume()
                      .WithEndpointProxySupport(false)
                      .WithLifetime(ContainerLifetime.Persistent);
#pragma warning restore ASPIREPROXYENDPOINTS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var db = dbServer.AddDatabase("PatientApp");

var userService = builder.AddProject<Projects.UserService_API>("user-service")
                         .WithReference(db)
                         .WaitFor(db)
                         .WithUrlForEndpoint("https", url => {
                             url.DisplayText = "Scalar HTTPS";
                             url.Url = "/scalar";
                         })
                         .WithEnvironment("Jwt__Issuer", jwtSection["Issuer"] ?? string.Empty)
                         .WithEnvironment("Jwt__Audience", jwtSection["Audience"] ?? string.Empty)
                         .WithEnvironment("Jwt__Key", jwtSection["Key"] ?? string.Empty)
                         .WithEnvironment("Jwt__ExpirationInMinutes", jwtSection["ExpirationInMinutes"] ?? string.Empty);


var patientService = builder.AddProject<Projects.PatientService_API>("patient-service")
                            .WithReference(db)
                            .WaitFor(db)
                            .WithUrlForEndpoint("https", url => {
                                url.DisplayText = "Scalar HTTPS";
                                url.Url = "/scalar";
                            })
                            .WithEnvironment("Jwt__Issuer", jwtSection["Issuer"] ?? string.Empty)
                            .WithEnvironment("Jwt__Audience", jwtSection["Audience"] ?? string.Empty)
                            .WithEnvironment("Jwt__Key", jwtSection["Key"] ?? string.Empty)
                            .WithEnvironment("Jwt__ExpirationInMinutes", jwtSection["ExpirationInMinutes"] ?? string.Empty);


builder.Build().Run();
