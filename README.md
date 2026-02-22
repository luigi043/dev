# InsureX - Insurance Asset Protection & Compliance Platform

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com)
[![Database](https://img.shields.io/badge/SQL%20Server-2019+-red)](https://www.microsoft.com/sql-server)
[![Build](https://img.shields.io/badge/build-passing-brightgreen)]()
[![PRs](https://img.shields.io/badge/PRs-welcome-brightgreen)](https://github.com/luigi043/InsureX/pulls)

## 📋 Overview

**InsureX** is a comprehensive insurance asset management and compliance platform designed for the modern B2B insurance landscape. The system enables financers, insurers, and policyholders to track insured assets, manage policies, and maintain regulatory compliance through near-real-time monitoring and workflow orchestration.

## 🚦 Current Status (February 2026)

| Module | Status | Progress | Notes |
|--------|--------|----------|-------|
| **Tenant & Identity** | ✅ Complete | 100% | Multi-tenant with ASP.NET Core Identity |
| **Asset Registry** | ✅ Complete | 100% | Full CRUD with repository pattern |
| **Policy Management** | 🚧 In Progress | 60% | Basic structure, needs UI |
| **Compliance Engine** | 🚧 In Progress | 40% | Core entities defined, needs implementation |
| **Workflow** | ⏳ Planned | 0% | Not started |
| **Audit & Evidence** | 🚧 In Progress | 30% | Base audit structure in place |
| **Integrations** | ⏳ Planned | 0% | Not started |

### Recent Fixes (Feb 22, 2026)
- ✅ Fixed duplicate interface definitions
- ✅ Added missing compliance entity classes
- ✅ Resolved package version conflicts
- ✅ Cleaned up legacy solution files
- ✅ Updated Moq package to fix security vulnerability


## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server)
- [Visual Studio 2022](https://visualstudio.microsoft.com) or [VS Code](https://code.visualstudio.com)
- [Git](https://git-scm.com)

### Installation

```bash
# Clone the repository
git clone https://github.com/luigi043/InsureX.git
cd InsureX

# Restore dependencies
dotnet restore

# Update database
dotnet ef database update --project src/InsureX.Infrastructure --startup-project src/InsureX.Web

# Run the application
dotnet run --project src/InsureX.Web
```

Access the application at: `https://localhost:5001`

Default login credentials:
- **Admin**: admin@insurex.com / Admin123!
- **Financer**: financer@insurex.com / Financer123!
- **Insurer**: insurer@insurex.com / Insurer123!

## 🏗 Architecture

The project follows Clean Architecture principles with clear separation of concerns:

```
InsureX/
├── src/
│   ├── InsureX.Web/           # ASP.NET Core 8 MVC Frontend
│   ├── InsureX.Api/            # ASP.NET Core 8 Web API
│   ├── InsureX.Application/     # Business Logic Layer
│   ├── InsureX.Domain/          # Core Domain Layer
│   ├── InsureX.Infrastructure/  # Data Access Layer
│   └── Modules/                  # Razor Class Libraries
└── tests/
    ├── InsureX.UnitTests/
    └── InsureX.IntegrationTests/
```

## ✨ Features

### Implemented ✅
- Multi-tenant architecture with TenantId isolation
- ASP.NET Core Identity with role-based access control
- Complete Asset Management CRUD operations
- Secure cookie authentication with HTTPS
- Repository pattern with Unit of Work
- AutoMapper for object mapping
- FluentValidation for request validation
- Serilog for structured logging
- Swagger/OpenAPI documentation

### In Progress 🚧
- Policy Management module
- Compliance monitoring engine
- Audit logging system
- Integration tests

## 📚 Documentation

- [API Documentation](https://localhost:5002/swagger) (when running)
- [Database Schema](docs/database.md)
- [Contributing Guide](CONTRIBUTING.md)

## 🧪 Testing

```bash
# Run unit tests
dotnet test tests/InsureX.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🚢 Deployment

### Docker
```bash
docker-compose up -d
```

### Azure
See [deployment guide](docs/azure-deployment.md) for detailed instructions.

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/luigi043/InsureX/issues)
- **Email**: support@insurex.com

---

**Maintained by**: Luigi & Team  
**Last Updated**: February 22, 2026  
**Current Version**: 2.0.0
```

## ✅ **Final Checklist**

- [ ] Repository cleaned of legacy files
- [ ] New solution file created
- [ ] Missing classes added (NotFoundException, PagedResult, TenantContext)
- [ ] Views created (Index, _AssetTable, Create)
- [ ] Program.cs properly configured
- [ ] Seed data created
- [ ] appsettings.json files configured
- [ ] Database migration created and applied
- [ ] .gitignore updated
- [ ] README.md updated
- [ ] Project builds successfully
- [ ] Application runs and allows login
- [ ] Asset CRUD operations work
