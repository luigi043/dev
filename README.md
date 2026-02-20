# InsureX - Insurance Compliance Platform

##  Architecture

Clean Architecture with:
- **ASP.NET Core 8** (MVC + Web API)
- **Entity Framework Core** with SQL Server
- **Multi-tenancy** support with TenantId isolation
- **Secure Authentication** with ASP.NET Core Identity
- **Repository Pattern** with Unit of Work
- **Azure-ready** (Key Vault, Service Bus, App Services)

##  Project Structure

```
InsureX/
├── src/
│   ├── InsureX.Web/          # MVC Frontend
│   ├── InsureX.Api/           # Web API
│   ├── InsureX.Application/   # Business Logic
│   ├── InsureX.Domain/        # Core Entities
│   └── InsureX.Infrastructure/ # Data Access
└── tests/
    ├── InsureX.UnitTests/
    └── InsureX.IntegrationTests/
```

##  Getting Started

1. Clone the repository
2. Update connection string in `appsettings.json`
3. Run migrations: `dotnet ef database update`
4. Run the app: `dotnet run --project src/InsureX.Web`

## Security Features

- ASP.NET Core Identity with password policies
- Secure cookie configuration (HttpOnly, Secure, SameSite)
- Multi-tenancy data isolation
- Azure Key Vault ready for secrets

##  Database

- SQL Server with Entity Framework Core
- Global query filters for tenant isolation
- Audit logging ready
- Migration scripts included
