# InsureX - Insurance Asset Protection & Compliance Platform

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2%20%7C%20.NET%208.0-blue)](https://dotnet.microsoft.com)
[![Status](https://img.shields.io/badge/status-modernization%20in%20progress-orange)]()
[![Azure](https://img.shields.io/badge/azure-ready-blue)]()

## 📋 Overview

**InsureX** is a comprehensive insurance asset management and compliance platform designed for the modern B2B insurance landscape. The system enables financers, insurers, and policyholders to track insured assets, manage policies, and maintain regulatory compliance through near-real-time monitoring and workflow orchestration.

### 🎯 Current State & Vision

We are currently modernizing InsureX from a legacy WebForms system to a cloud-native, multi-tenant SaaS platform. The codebase represents both the current production system and our target architecture.

| Aspect | Current (Legacy) | Target (Modern) |
|--------|------------------|-----------------|
| **Architecture** | WebForms + WCF | ASP.NET Core 8 + Clean Architecture |
| **Tenancy** | Partner segmentation | True multi-tenant with isolation |
| **Security** | Custom auth + reversible encryption | ASP.NET Core Identity + Azure Key Vault |
| **Integration** | Stubbed WCF endpoints | Event-driven + Azure Service Bus |
| **Deployment** | On-premise | Azure-native (App Services + SQL) |

## 🏗 Architecture

### Current Structure (Legacy)
```
Insurex_New/
├── IAPR_Web/                 # WebForms Presentation Layer
├── IAPR_API/                  # WCF/REST Service Layer
├── IAPR_Data/                 # Data Access Layer
└── InsurexService/             # Email Notification Service
```

### Target Architecture (Modern)
```
InsureX/
├── src/
│   ├── InsureX.Web/           # ASP.NET Core 8 MVC (Razor + jQuery)
│   ├── InsureX.Api/            # ASP.NET Core 8 Web API
│   ├── InsureX.Application/     # Business Logic / Services
│   ├── InsureX.Domain/          # Core Entities & Interfaces
│   ├── InsureX.Infrastructure/  # EF Core, Repositories, Integrations
│   └── Modules/                 # Razor Class Libraries
│       ├── InsureX.Ui.Shell.Rcl/
│       ├── InsureX.Ui.Compliance.Rcl/
│       └── InsureX.Ui.Workflow.Rcl/
└── tests/
    ├── InsureX.UnitTests/
    └── InsureX.IntegrationTests/
```

## ✨ Key Features

### Core Capabilities
- **Multi-tenant Portal** - Role-based access for Admins, Financers, Insurers
- **Asset Registry** - Complete lifecycle management for financed assets
- **Policy Management** - Coverage tracking and compliance monitoring
- **Compliance Engine** - Near-real-time compliance detection
- **Workflow Orchestration** - Case management with SLA tracking
- **Audit Trail** - Immutable event logging for regulatory compliance

### Technical Features
- 🔐 **Multi-tenancy** - TenantId isolation with Row-Level Security
- 🚀 **Event-driven** - Azure Service Bus for reliable processing
- 📊 **Real-time UI** - Razor partials + jQuery AJAX updates
- 🔄 **Idempotent APIs** - Safe retries with idempotency keys
- 📈 **Pagination** - Standardized paging across all endpoints
- 🔍 **Observability** - Correlation IDs + Application Insights

## 🚀 Getting Started

### Prerequisites

#### For Legacy Development
- Visual Studio 2019/2022
- .NET Framework 4.7.2 SDK
- SQL Server (LocalDB or full instance)

#### For Modern Development
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019+
- Azure Subscription (optional, for cloud deployment)
- Visual Studio 2022 / VS Code

### Quick Start (Modern Stack)

```bash
# Clone the repository
git clone https://github.com/luigi043/InsureX.git
cd InsureX

# Restore dependencies
dotnet restore

# Update database (update connection string in appsettings.json first)
dotnet ef database update --project src/InsureX.Infrastructure

# Run the web application
dotnet run --project src/InsureX.Web

# Run the API (in a separate terminal)
dotnet run --project src/InsureX.Api
```

Access the applications:
- Web Portal: `https://localhost:5001`
- API: `https://localhost:5002/swagger`

### Legacy System Setup

If you need to work with the legacy codebase:

```bash
cd Insurex_New
# Open the solution in Visual Studio
start Insured_Assest_Protection_Register.sln
```

## 📚 Module Documentation

### Core Modules

| Module | Description | Status |
|--------|-------------|--------|
| **Tenant & Identity** | Multi-tenant user management with ASP.NET Core Identity | 🚧 In Progress |
| **Asset Registry** | Asset lifecycle management | ✅ Legacy |
| **Policy Management** | Insurance policy administration | ✅ Legacy |
| **Compliance Engine** | Real-time compliance monitoring | 🚧 In Progress |
| **Workflow** | Case management with SLA tracking | 🚧 In Progress |
| **Audit & Evidence** | Immutable audit logging | 🚧 In Progress |
| **Integrations** | Insurer/Bank API connectors | 🚧 In Progress |

### API Endpoints (Modern)

```
GET    /api/v1/assets                    # List assets (paged)
POST   /api/v1/assets                     # Create asset
GET    /api/v1/assets/{id}                 # Get asset details
GET    /api/v1/assets/{id}/compliance      # Get compliance status
POST   /api/v1/integrations/insurers/webhook  # Insurer webhook
GET    /api/v1/cases                       # List compliance cases
POST   /api/v1/cases/{id}/actions/escalate # Escalate case
```

## 🔧 Configuration

### Database Connection (Modern)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InsureX;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Azure": {
    "KeyVaultUrl": "https://insurex-kv.vault.azure.net/",
    "ServiceBusConnection": "Endpoint=sb://..."
  }
}
```

### Security Settings
```json
{
  "Authentication": {
    "Cookie": {
      "HttpOnly": true,
      "SecurePolicy": "Always",
      "SameSite": "Lax"
    }
  }
}
```

## 🛡 Security Features

### Implemented
- ✅ ASP.NET Core Identity with password policies
- ✅ Secure cookie configuration (HttpOnly, Secure, SameSite)
- ✅ Multi-tenancy data isolation with TenantId
- ✅ Azure Key Vault integration for secrets
- ✅ HTTPS enforcement

### In Progress
- 🚧 Row-Level Security in SQL Server
- 🚧 OAuth2 Client Credentials for system integrations
- 🚧 Signed webhook verification (HMAC)
- 🚧 Audit logging for all state changes

## 📊 Database Schema

The modern architecture uses a shared database with TenantId isolation:

```sql
-- Tenant-scoped tables include TenantId
CREATE TABLE Assets (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    -- other columns...
)

-- Row-Level Security enabled
CREATE SECURITY POLICY TenantPolicy
    ADD FILTER PREDICATE TenantPredicate(TenantId) ON Assets
```

## 🧪 Testing

### Unit Tests
```bash
dotnet test tests/InsureX.UnitTests
```

### Integration Tests
```bash
dotnet test tests/InsureX.IntegrationTests
```

### Manual Testing (Legacy)
Test accounts:
- Admin: `admin@insurex.com` / Admin123!
- Financer: `financer@imobiliaria.com` / Financer123!
- Insurer: `insurer@segurodirecto.com` / Insurer123!

## 🚢 Deployment (Azure)

### Infrastructure as Code
```bash
# Deploy using Azure CLI / Bicep
az deployment group create --resource-group InsureX-RG --template-file infra/main.bicep
```

### CI/CD Pipeline (GitHub Actions)
- Automated builds and tests
- EF Core migrations
- Blue-green deployments to App Service slots
- Integration with Azure Key Vault

## 📈 Roadmap

### Phase 0: Emergency Security (Immediate)
- [ ] Rotate exposed credentials
- [ ] Remove secrets from configs
- [ ] Add secure cookie settings

### Phase 1: Foundation (Q1 2026)
- [ ] Complete multi-tenancy implementation
- [ ] Migrate to ASP.NET Core Identity
- [ ] Implement audit logging
- [ ] Add comprehensive test suite

### Phase 2: Modernization (Q2 2026)
- [ ] Convert WebForms to Razor Pages
- [ ] Implement event-driven architecture
- [ ] Add Azure Service Bus integration
- [ ] Build compliance engine

### Phase 3: Advanced Features (Q3 2026)
- [ ] Real-time insurer integrations
- [ ] Machine learning for fraud detection
- [ ] Advanced reporting dashboard
- [ ] Mobile app support

## 🤝 Contributing

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Coding Standards
- Follow C# coding conventions
- Add XML comments for public APIs
- Write unit tests for new features
- Update documentation
- Ensure tenant isolation in all queries

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Original development team at InsureX
- Contributors and reviewers
- Open-source community

## 📞 Contact & Support

- **Documentation**: [https://docs.insurex.com](https://docs.insurex.com)
- **Issues**: [GitHub Issues](https://github.com/luigi043/InsureX/issues)
- **Security Issues**: security@insurex.com

---

**Maintained by**: Luigi & Team  
**Last Updated**: February 2026  
**Current Version**: 2.0.0 (Modernization in Progress)

---