# GitHub Copilot Instructions for PatientApp

## Project Overview

PatientApp is an AI-driven Patient Management System (PMS) designed as a SaaS, multi-tenant web application. The solution is built on a complete Microsoft stack and follows modern cloud-native architecture principles.

## Technology Stack

### Core Technologies
- **Backend**: ASP.NET Core (.NET 8+)
- **ORM**: Entity Framework Core
- **Database**: SQL Server (Azure SQL)
- **Orchestration**: .NET Aspire for service discovery, configuration, and deployment
- **API Gateway**: YARP (Yet Another Reverse Proxy)
- **AI Integration**: Microsoft Semantic Kernel with Azure OpenAI
- **Authentication**: ASP.NET Identity with JWT tokens
- **Testing**: xUnit, FluentAssertions
- **API Documentation**: Scalar for OpenAPI/Swagger

### Azure Services
- Azure SQL Database
- Azure Blob Storage for document management
- Azure Service Bus for messaging
- Azure Monitor and Application Insights
- Azure OpenAI for AI features

## Architecture Principles

### Microservices
- The application follows a microservices architecture
- Each service should be independently deployable
- Services include: UserService, PatientService, AppointmentService, BillingService
- Use Aspire for orchestration and service discovery
- Use YARP as the centralized API gateway for routing and security

### Multi-Tenancy
- **Critical**: All data must be tenant-aware
- Every entity should include a `TenantId` field where applicable
- Use EF Core global query filters to automatically filter by tenant
- All API endpoints must validate and enforce tenant context
- Users can belong to multiple tenants with context switching capability
- Prevent data leakage between tenants at all layers

### Security & Compliance
- Implement role-based access control (RBAC) for patients, clinicians, and admins
- Encrypt data at rest and in transit
- Enable audit logging for sensitive operations
- Ensure HIPAA/GDPR compliance considerations
- Use secure authentication with JWT tokens
- Validate all user input and sanitize outputs

## Coding Standards

### C# Conventions
- Use nullable reference types and enable nullable context
- Follow C# naming conventions (PascalCase for classes/methods, camelCase for parameters/locals)
- Use primary constructors for dependency injection where appropriate
- Prefer `record` types for DTOs and value objects
- Use `sealed` keyword for classes not designed for inheritance
- Use expression-bodied members for simple methods and properties
- Use file-scoped namespaces to reduce indentation

### API Development
- Use FastEndpoints for building minimal APIs with good separation of concerns
- Create separate validator classes using FluentValidation
- Return typed results (e.g., `Results<Ok<T>, BadRequest>`)
- Use proper HTTP status codes
- Document APIs with OpenAPI/Swagger annotations
- Implement proper error handling and logging

### Database & EF Core
- Use Code First approach with migrations
- Apply global query filters for multi-tenancy
- Use async/await for all database operations
- Implement proper indexes for query performance
- Use value converters for complex types
- Follow repository pattern where appropriate
- Name migrations descriptively (e.g., `AddPatientTable`, `UpdateUserSchema`)

### Dependency Injection
- Register services in `Program.cs` with appropriate lifetimes
- Use constructor injection for dependencies
- Follow the dependency inversion principle
- Prefer interfaces for service contracts

### Logging
- Use structured logging with `ILogger<T>`
- Log at appropriate levels (Debug, Information, Warning, Error, Critical)
- Include relevant context in log messages
- Use log scopes for correlation

### Testing
- Write unit tests for business logic
- Use integration tests for API endpoints and database operations
- Follow AAA pattern (Arrange, Act, Assert)
- Use FluentAssertions for readable assertions
- Mock external dependencies
- Test multi-tenant data isolation

## AI & Semantic Kernel Guidelines

### Semantic Kernel Integration
- Use Semantic Kernel for semantic search across patient records
- Implement AI chatbot for patient queries and appointment booking
- Generate clinician insights using context-aware AI prompts
- Enable natural language queries for reporting
- Ensure AI features respect tenant boundaries

### AI Best Practices
- Validate and sanitize AI inputs and outputs
- Implement rate limiting for AI endpoints
- Handle AI service failures gracefully
- Log AI interactions for audit purposes
- Ensure patient data privacy in AI features

## Project Structure

```
/src
  /PatientApp.Aspire.AppHost        # Aspire orchestration
  /PatientApp.Aspire.ServiceDefaults # Shared Aspire configuration
  /api
    /UserService                     # User authentication and management
    /PatientService                  # Patient data and records
    /AppointmentService              # Appointment scheduling
    /BillingService                  # Billing and invoicing
  /gateway                           # YARP API Gateway
  /web                              # Frontend application
/tests
  /UserService.Tests.Integration     # Integration tests
  /[Service].Tests.Unit             # Unit tests
```

## Common Patterns

### Service Registration
```csharp
builder.AddServiceDefaults(); // Aspire defaults
builder.AddSqlServerDbContext<AppDbContext>("DatabaseName");
builder.Services.AddFastEndpoints();
builder.Services.AddOpenApi();
```

### FastEndpoints Pattern
```csharp
internal sealed class MyEndpoint : Endpoint<MyRequest, Results<Ok<MyResponse>, BadRequest>>
{
    public override void Configure()
    {
        Post("/api/resource");
        Roles("User", "Admin"); // or AllowAnonymous()
    }

    public override async Task HandleAsync(MyRequest req, CancellationToken ct)
    {
        // Implementation
    }
}
```

### Tenant-Aware Queries
```csharp
// Apply global query filter in DbContext
modelBuilder.Entity<Patient>()
    .HasQueryFilter(p => p.TenantId == _currentTenant.Id);
```

## Documentation
- Update README.md with setup instructions
- Document API endpoints with OpenAPI/Swagger
- Add XML comments for public APIs
- Maintain architecture decision records (ADRs) for significant decisions

## Environment Configuration
- Use Aspire for environment-specific configuration
- Store secrets in Azure Key Vault or user secrets
- Configure JWT settings in appsettings.json
- Use environment variables for deployment configuration

## Performance Considerations
- Use async/await throughout
- Implement caching where appropriate (distributed cache for multi-tenant)
- Optimize database queries with proper indexes
- Use pagination for large data sets
- Consider connection pooling for database access

## Error Handling
- Use problem details for API errors
- Implement global exception handling
- Log errors with full context
- Return user-friendly error messages
- Handle specific exceptions appropriately

## When Generating Code
- Always consider multi-tenancy requirements
- Include proper error handling and logging
- Follow the established patterns in the codebase
- Write tests alongside implementation
- Update documentation as needed
- Consider security implications
- Ensure HIPAA compliance for patient data
