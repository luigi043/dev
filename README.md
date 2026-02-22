# InsureX - Insurance Asset Protection & Compliance Platform

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-purple)](https://learn.microsoft.com/ef/core/)
[![Azure](https://img.shields.io/badge/azure-ready-0089D6)](https://azure.microsoft.com)
[![Status](https://img.shields.io/badge/status-active%20development-brightgreen)]()
[![PRs](https://img.shields.io/badge/PRs-welcome-brightgreen)](https://github.com/luigi043/InsureX/pulls)

## 📋 Overview

**InsureX** is a comprehensive insurance asset management and compliance platform designed for the modern B2B insurance landscape. The system enables financers, insurers, and policyholders to track insured assets, manage policies, and maintain regulatory compliance through near-real-time monitoring and workflow orchestration.

### 🎯 Current Status - February 2026

| Area | Status | Progress |
|------|--------|----------|
| **Modern Architecture** | ✅ Complete | 100% |
| **Asset Management** | ✅ Complete | 100% |
| **Authentication** | ✅ Complete | 100% |
| **Multi-tenancy** | 🚧 In Progress | 75% |
| **Policy Management** | 🚧 In Progress | 60% |
| **Compliance Engine** | ⏳ Planned | 0% |
| **API Integrations** | ⏳ Planned | 0% |

## 🏗 Architecture

### Solution Structure
```
InsureX/
├── 📁 src/
│   ├── 📁 InsureX.Web/           # ASP.NET Core 8 MVC Frontend
│   │   ├── 📁 Controllers/
│   │   ├── 📁 Views/
│   │   ├── 📁 wwwroot/
│   │   └── Program.cs
│   │
│   ├── 📁 InsureX.Api/            # ASP.NET Core 8 Web API
│   │   ├── 📁 Controllers/
│   │   ├── 📁 Middleware/
│   │   └── Program.cs
│   │
│   ├── 📁 InsureX.Application/     # Business Logic Layer
│   │   ├── 📁 DTOs/
│   │   ├── 📁 Interfaces/
│   │   ├── 📁 Services/
│   │   └── 📁 Validators/
│   │
│   ├── 📁 InsureX.Domain/          # Core Domain Layer
│   │   ├── 📁 Entities/
│   │   ├── 📁 Enums/
│   │   └── 📁 Interfaces/
│   │
│   ├── 📁 InsureX.Infrastructure/  # Data Access Layer
│   │   ├── 📁 Data/
│   │   ├── 📁 Repositories/
│   │   ├── 📁 Migrations/
│   │   └── 📁 Extensions/
│   │
│   └── 📁 Modules/                  # Razor Class Libraries
│       ├── 📁 InsureX.Ui.Shell.Rcl/
│       ├── 📁 InsureX.Ui.Compliance.Rcl/
│       └── 📁 InsureX.Ui.Workflow.Rcl/
│
└── 📁 tests/
    ├── 📁 InsureX.UnitTests/
    └── 📁 InsureX.IntegrationTests/
```

## ✨ Key Features

### Core Capabilities
- 🔐 **Multi-tenant Portal** - Role-based access for Admins, Financers, Insurers
- 📦 **Asset Registry** - Complete lifecycle management for financed assets
- 📋 **Policy Management** - Coverage tracking and compliance monitoring
- ⚡ **Compliance Engine** - Near-real-time compliance detection
- 🔄 **Workflow Orchestration** - Case management with SLA tracking
- 📝 **Audit Trail** - Immutable event logging for regulatory compliance

### Technical Features
- ✅ **Clean Architecture** - Separation of concerns with Domain-Driven Design
- ✅ **Repository Pattern** - Abstracted data access with Unit of Work
- ✅ **Multi-tenancy** - TenantId isolation with Global Query Filters
- ✅ **ASP.NET Core Identity** - Secure authentication with password policies
- ✅ **JWT Authentication** - Token-based API authentication
- ✅ **AutoMapper** - Object-object mapping
- ✅ **FluentValidation** - Request validation
- ✅ **Serilog** - Structured logging
- ✅ **Swagger/OpenAPI** - API documentation

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server 2019+](https://www.microsoft.com/sql-server)
- [Visual Studio 2022](https://visualstudio.microsoft.com) or [VS Code](https://code.visualstudio.com)
- [Git](https://git-scm.com)

### Quick Start

```bash
# 1. Clone the repository
git clone https://github.com/luigi043/InsureX.git
cd InsureX

# 2. Restore dependencies
dotnet restore

# 3. Update database connection string
# Edit appsettings.json in InsureX.Web and InsureX.Api
```

**appsettings.json configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InsureX_DB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "Secret": "YOUR-SECRET-KEY-HERE-MINIMUM-32-CHARS",
    "ExpirationHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

```bash
# 4. Create and update database
dotnet ef migrations add InitialCreate --project src/InsureX.Infrastructure --startup-project src/InsureX.Web
dotnet ef database update --project src/InsureX.Infrastructure --startup-project src/InsureX.Web

# 5. Run the applications (in separate terminals)
# Terminal 1 - Web App
cd src/InsureX.Web
dotnet run
# Access at: https://localhost:5001

# Terminal 2 - API
cd src/InsureX.Api
dotnet run
# Access at: https://localhost:5002/swagger
```

## 💻 Code Examples

### Domain Entity Example
```csharp
// InsureX.Domain/Entities/Asset.cs
namespace InsureX.Domain.Entities;

public class Asset : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public AssetStatus Status { get; set; }
    public DateTime PurchaseDate { get; set; }
    
    // Navigation properties
    public virtual Tenant Tenant { get; set; }
    public virtual ICollection<Policy> Policies { get; set; }
}

public enum AssetStatus
{
    Active = 1,
    Inactive = 2,
    Sold = 3,
    Stolen = 4,
    Totaled = 5
}
```

### Repository Pattern
```csharp
// InsureX.Domain/Interfaces/IRepository.cs
namespace InsureX.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}

// InsureX.Infrastructure/Repositories/Repository.cs
namespace InsureX.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }
}
```

### Service Layer Example
```csharp
// InsureX.Application/DTOs/AssetDto.cs
namespace InsureX.Application.DTOs;

public class AssetDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
}

public class CreateAssetDto
{
    [Required]
    [StringLength(50)]
    public string AssetTag { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Make { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Model { get; set; } = string.Empty;
    
    [Range(1900, 2100)]
    public int Year { get; set; }
    
    [Required]
    public string SerialNumber { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal Value { get; set; }
    
    public DateTime PurchaseDate { get; set; }
}

// InsureX.Application/Interfaces/IAssetService.cs
namespace InsureX.Application.Interfaces;

public interface IAssetService
{
    Task<AssetDto?> GetByIdAsync(Guid id);
    Task<PagedResult<AssetDto>> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
    Task<AssetDto> CreateAsync(CreateAssetDto dto);
    Task UpdateAsync(Guid id, CreateAssetDto dto);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string assetTag);
}

// InsureX.Application/Services/AssetService.cs
namespace InsureX.Application.Services;

public class AssetService : IAssetService
{
    private readonly IRepository<Asset> _assetRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;

    public AssetService(
        IRepository<Asset> assetRepository,
        ITenantContext tenantContext,
        IMapper mapper)
    {
        _assetRepository = assetRepository;
        _tenantContext = tenantContext;
        _mapper = mapper;
    }

    public async Task<AssetDto?> GetByIdAsync(Guid id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        return asset == null ? null : _mapper.Map<AssetDto>(asset);
    }

    public async Task<PagedResult<AssetDto>> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        var query = await _assetRepository.FindAsync(a => 
            a.TenantId == _tenantContext.TenantId);
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a => 
                a.AssetTag.Contains(searchTerm) || 
                a.SerialNumber.Contains(searchTerm));
        }

        var totalItems = query.Count();
        var items = query
            .OrderBy(a => a.AssetTag)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<AssetDto>
        {
            Items = _mapper.Map<List<AssetDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            HasNext = page * pageSize < totalItems
        };
    }

    public async Task<AssetDto> CreateAsync(CreateAssetDto dto)
    {
        var asset = _mapper.Map<Asset>(dto);
        asset.TenantId = _tenantContext.TenantId.Value;
        
        var created = await _assetRepository.AddAsync(asset);
        return _mapper.Map<AssetDto>(created);
    }

    public async Task UpdateAsync(Guid id, CreateAssetDto dto)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null || asset.TenantId != _tenantContext.TenantId)
            throw new NotFoundException($"Asset with ID {id} not found");

        _mapper.Map(dto, asset);
        await _assetRepository.UpdateAsync(asset);
    }

    public async Task DeleteAsync(Guid id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null || asset.TenantId != _tenantContext.TenantId)
            throw new NotFoundException($"Asset with ID {id} not found");

        await _assetRepository.DeleteAsync(asset);
    }

    public async Task<bool> ExistsAsync(string assetTag)
    {
        var assets = await _assetRepository.FindAsync(a => 
            a.AssetTag == assetTag && 
            a.TenantId == _tenantContext.TenantId);
        return assets.Any();
    }
}
```

### Controller Example
```csharp
// InsureX.Web/Controllers/AssetsController.cs
namespace InsureX.Web.Controllers;

[Authorize]
public class AssetsController : Controller
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 25, string? searchTerm = null)
    {
        var model = await _assetService.GetPagedAsync(page, pageSize, searchTerm);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(int page = 1, string? searchTerm = null)
    {
        var model = await _assetService.GetPagedAsync(page, 25, searchTerm);
        return PartialView("_AssetTable", model);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAssetDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var asset = await _assetService.CreateAsync(dto);
            TempData["Success"] = "Asset created successfully";
            return RedirectToAction(nameof(Details), new { id = asset.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while creating the asset");
            return View(dto);
        }
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        return View(asset);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        var dto = _mapper.Map<CreateAssetDto>(asset);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateAssetDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _assetService.UpdateAsync(id, dto);
            TempData["Success"] = "Asset updated successfully";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An error occurred while updating the asset");
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _assetService.DeleteAsync(id);
            TempData["Success"] = "Asset deleted successfully";
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while deleting the asset";
        }

        return RedirectToAction(nameof(Index));
    }
}
```

### Razor View with AJAX
```html
<!-- InsureX.Web/Views/Assets/Index.cshtml -->
@model PagedResult<AssetDto>

@{
    ViewData["Title"] = "Assets";
}

<div class="container-fluid">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>Asset Registry</h1>
        <a asp-action="Create" class="btn btn-primary">
            <i class="fas fa-plus"></i> New Asset
        </a>
    </div>

    <!-- Search Form -->
    <div class="card shadow mb-4">
        <div class="card-body">
            <form id="searchForm" method="post" asp-action="Search">
                <div class="row">
                    <div class="col-md-6">
                        <div class="input-group">
                            <input type="text" 
                                   name="searchTerm" 
                                   class="form-control" 
                                   placeholder="Search by Asset Tag or Serial Number..." 
                                   value="@Context.Request.Query["searchTerm"]" />
                            <button type="submit" class="btn btn-primary">
                                <i class="fas fa-search"></i> Search
                            </button>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    </div>

    <!-- Asset Table Container -->
    <div id="assetTableContainer">
        @await Html.PartialAsync("_AssetTable", Model)
    </div>
</div>

@section Scripts {
    <script>
        $(document).ready(function() {
            // Handle search form submission
            $('#searchForm').submit(function(e) {
                e.preventDefault();
                
                $.ajax({
                    url: '@Url.Action("Search", "Assets")',
                    type: 'POST',
                    data: $(this).serialize(),
                    success: function(result) {
                        $('#assetTableContainer').html(result);
                    },
                    error: function() {
                        toastr.error('Error searching assets');
                    }
                });
            });

            // Handle pagination clicks
            $(document).on('click', '.page-link', function(e) {
                e.preventDefault();
                
                var page = $(this).data('page');
                var searchTerm = $('input[name="searchTerm"]').val();
                
                $.ajax({
                    url: '@Url.Action("Search", "Assets")',
                    type: 'POST',
                    data: { page: page, searchTerm: searchTerm },
                    success: function(result) {
                        $('#assetTableContainer').html(result);
                    }
                });
            });
        });
    </script>
}
```

### API Controller
```csharp
// InsureX.Api/Controllers/V1/AssetsController.cs
namespace InsureX.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AssetDto>>> GetAssets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _assetService.GetPagedAsync(page, pageSize, searchTerm);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetDto>> GetAsset(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        return Ok(asset);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssetDto>> CreateAsset(CreateAssetDto dto)
    {
        var asset = await _assetService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsset(Guid id, CreateAssetDto dto)
    {
        try
        {
            await _assetService.UpdateAsync(id, dto);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsset(Guid id)
    {
        try
        {
            await _assetService.DeleteAsync(id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
```

### Multi-tenancy Middleware
```csharp
// InsureX.Api/Middleware/TenantMiddleware.cs
namespace InsureX.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Extract tenant from JWT token
        var tenantIdClaim = context.User.FindFirst("tenant_id");
        
        if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            tenantContext.SetTenantId(tenantId);
        }
        else
        {
            // For anonymous endpoints, you might handle differently
            // Or return 401 for protected endpoints
        }

        await _next(context);
    }
}

// Extension method for easy registration
public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
```

### Program.cs Configuration
```csharp
// InsureX.Web/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add services and repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Add HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
```

## 📚 Module Documentation

### Core Modules Status

| Module | Description | Status | Progress |
|--------|-------------|--------|----------|
| **Tenant & Identity** | Multi-tenant user management with ASP.NET Core Identity | ✅ Complete | 100% |
| **Asset Registry** | Complete asset lifecycle management | ✅ Complete | 100% |
| **Policy Management** | Insurance policy administration | 🚧 In Progress | 60% |
| **Compliance Engine** | Real-time compliance monitoring | ⏳ Planned | 0% |
| **Workflow** | Case management with SLA tracking | ⏳ Planned | 0% |
| **Audit & Evidence** | Immutable audit logging | 🚧 In Progress | 30% |
| **Integrations** | Insurer/Bank API connectors | ⏳ Planned | 0% |

### API Endpoints

#### Assets API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/assets` | List all assets (paged) |
| GET | `/api/v1/assets/{id}` | Get asset by ID |
| POST | `/api/v1/assets` | Create new asset |
| PUT | `/api/v1/assets/{id}` | Update asset |
| DELETE | `/api/v1/assets/{id}` | Delete asset |

#### Policies API (Coming Soon)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/policies` | List all policies |
| GET | `/api/v1/assets/{id}/policies` | Get policies for asset |
| POST | `/api/v1/policies` | Create new policy |

## 🛡 Security Features

### Implemented ✅
- ASP.NET Core Identity with password policies
- Secure cookie configuration (HttpOnly, Secure, SameSite)
- JWT Bearer token authentication for API
- Multi-tenancy data isolation with TenantId
- HTTPS enforcement
- Anti-forgery tokens for forms
- CORS policy configuration

### In Progress 🚧
- Row-Level Security in SQL Server
- Audit logging for all state changes
- API rate limiting
- IP whitelisting for admin endpoints

### Planned ⏳
- OAuth2 Client Credentials for system integrations
- Signed webhook verification (HMAC)
- Azure Key Vault integration
- DDoS protection

## 📊 Database Schema

### Core Tables
```sql
-- Tenants table
CREATE TABLE Tenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Subdomain NVARCHAR(100) UNIQUE,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1
);

-- Users table (ASP.NET Core Identity)
CREATE TABLE AspNetUsers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    -- ... Identity columns ...
    CONSTRAINT FK_AspNetUsers_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

-- Assets table
CREATE TABLE Assets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AssetTag NVARCHAR(50) NOT NULL,
    Make NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    Year INT NOT NULL,
    SerialNumber NVARCHAR(100) NOT NULL,
    Value DECIMAL(18,2) NOT NULL,
    Status INT NOT NULL,
    PurchaseDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    CONSTRAINT FK_Assets_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

-- Create unique constraint for AssetTag per tenant
CREATE UNIQUE INDEX UX_Assets_TenantId_AssetTag 
    ON Assets(TenantId, AssetTag);

-- Create indexes for performance
CREATE INDEX IX_Assets_TenantId_Status ON Assets(TenantId, Status);
CREATE INDEX IX_Assets_TenantId_SerialNumber ON Assets(TenantId, SerialNumber);
```

### Row-Level Security
```sql
-- Enable RLS on tenant-scoped tables
ALTER TABLE Assets ENABLE ROW LEVEL SECURITY;

-- Create predicate function
CREATE FUNCTION dbo.fn_tenant_access_predicate(@TenantId UNIQUEIDENTIFIER)
    RETURNS TABLE
    WITH SCHEMABINDING
AS
    RETURN SELECT 1 AS access_result
    WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS UNIQUEIDENTIFIER);

-- Create security policy
CREATE SECURITY POLICY dbo.TenantAccessPolicy
    ADD FILTER PREDICATE dbo.fn_tenant_access_predicate(TenantId) ON dbo.Assets,
    ADD BLOCK PREDICATE dbo.fn_tenant_access_predicate(TenantId) ON dbo.Assents;
```

## 🧪 Testing

### Running Tests
```bash
# Unit tests
dotnet test tests/InsureX.UnitTests

# Integration tests
dotnet test tests/InsureX.IntegrationTests

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Example
```csharp
// tests/InsureX.UnitTests/Services/AssetServiceTests.cs
public class AssetServiceTests
{
    private readonly Mock<IRepository<Asset>> _mockRepo;
    private readonly Mock<ITenantContext> _mockTenantContext;
    private readonly IMapper _mapper;
    private readonly AssetService _service;

    public AssetServiceTests()
    {
        _mockRepo = new Mock<IRepository<Asset>>();
        _mockTenantContext = new Mock<ITenantContext>();
        
        var config = new MapperConfiguration(cfg => 
            cfg.CreateMap<CreateAssetDto, Asset>());
        _mapper = config.CreateMapper();
        
        _service = new AssetService(
            _mockRepo.Object,
            _mockTenantContext.Object,
            _mapper);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesAsset()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _mockTenantContext.Setup(x => x.TenantId).Returns(tenantId);
        
        var dto = new CreateAssetDto
        {
            AssetTag = "AST-001",
            Make = "Toyota",
            Model = "Camry",
            Year = 2023,
            SerialNumber = "SN123456",
            Value = 25000m,
            PurchaseDate = DateTime.UtcNow
        };

        _mockRepo.Setup(x => x.AddAsync(It.IsAny<Asset>()))
            .ReturnsAsync((Asset a) => a);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.AssetTag, result.AssetTag);
        _mockRepo.Verify(x => x.AddAsync(It.Is<Asset>(a => 
            a.TenantId == tenantId)), Times.Once);
    }
}
```

## 🚢 Deployment

### Docker Support
```dockerfile
# Dockerfile for InsureX.Web
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/InsureX.Web/InsureX.Web.csproj", "src/InsureX.Web/"]
COPY ["src/InsureX.Application/InsureX.Application.csproj", "src/InsureX.Application/"]
COPY ["src/InsureX.Domain/InsureX.Domain.csproj", "src/InsureX.Domain/"]
COPY ["src/InsureX.Infrastructure/InsureX.Infrastructure.csproj", "src/InsureX.Infrastructure/"]
RUN dotnet restore "src/InsureX.Web/InsureX.Web.csproj"
COPY . .
WORKDIR "/src/src/InsureX.Web"
RUN dotnet build "InsureX.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InsureX.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InsureX.Web.dll"]
```

### Docker Compose
```yaml
version: '3.8'

services:
  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Password
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql

  insurex-web:
    build:
      context: .
      dockerfile: src/InsureX.Web/Dockerfile
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sql-server;Database=InsureX_DB;User=sa;Password=YourStrong!Password;TrustServerCertificate=true
    depends_on:
      - sql-server

  insurex-api:
    build:
      context: .
      dockerfile: src/InsureX.Api/Dockerfile
    ports:
      - "5002:80"
      - "5003:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sql-server;Database=InsureX_DB;User=sa;Password=YourStrong!Password;TrustServerCertificate=true
    depends_on:
      - sql-server

volumes:
  sql-data:
```

### Azure Deployment (Bicep)
```bicep
// infra/main.bicep
param environment string = 'dev'
param location string = resourceGroup().location

var appServicePlanName = 'insurex-plan-${environment}'
var webAppName = 'insurex-web-${environment}'
var apiAppName = 'insurex-api-${environment}'
var sqlServerName = 'insurex-sql-${environment}'
var sqlDatabaseName = 'InsureX_${environment}'

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: 'insurexadmin'
    administratorLoginPassword: '@insurex-Password-123'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = {
  name: sqlDatabaseName
  parent: sqlServer
  location: location
  sku: {
    name: 'Basic'
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2021-02-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${sqlDatabaseName};Authentication=Active Directory Default;'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment
        }
      ]
    }
  }
}

// API App
resource apiApp 'Microsoft.Web/sites@2021-02-01' = {
  name: apiAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${sqlDatabaseName};Authentication=Active Directory Default;'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment
        }
      ]
    }
  }
}

output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output apiAppUrl string = 'https://${apiApp.properties.defaultHostName}'
```

## 📈 Roadmap & Progress

### Phase 1: Foundation (Q1 2026) - Current
- [x] Solution architecture setup
- [x] Multi-tenancy implementation
- [x] ASP.NET Core Identity integration
- [x] Asset management CRUD
- [x] Unit test framework
- [ ] Audit logging (75%)
- [ ] Integration tests (50%)

### Phase 2: Modernization (Q2 2026)
- [ ] Policy management module
- [ ] Compliance engine foundation
- [ ] Azure Service Bus integration
- [ ] Real-time dashboard
- [ ] Email notification system

### Phase 3: Advanced Features (Q3 2026)
- [ ] Insurer API integrations
- [ ] Workflow orchestration
- [ ] Machine learning for fraud detection
- [ ] Mobile app support

## 🤝 Contributing

### Development Workflow
```bash
# 1. Fork and clone
git clone https://github.com/your-username/InsureX.git
cd InsureX

# 2. Create feature branch
git checkout -b feature/your-feature-name

# 3. Make changes and commit
git add .
git commit -m "feat: add new feature"

# 4. Push and create PR
git push origin feature/your-feature-name
```

### Coding Standards
- Follow Microsoft's C# coding conventions
- Use async/await pattern for I/O operations
- Add XML comments for public APIs
- Write unit tests for new features
- Ensure tenant isolation in all queries
- Use meaningful variable and method names

### Commit Message Format
```
<type>(<scope>): <subject>

Types: feat, fix, docs, style, refactor, test, chore
Scope: asset, policy, compliance, auth, api, ui

Example: feat(asset): add search functionality
```

## 📞 Contact & Support

- **Documentation**: [https://docs.insurex.com](https://docs.insurex.com)
- **Issues**: [GitHub Issues](https://github.com/luigi043/InsureX/issues)
- **Discussions**: [GitHub Discussions](https://github.com/luigi043/InsureX/discussions)
- **Email**: support@insurex.com

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Original development team at InsureX
- ASP.NET Core community
- Contributors and reviewers
- Open-source libraries used:
  - AutoMapper
  - FluentValidation
  - Serilog
  - xUnit

---

**Maintained by**: Luigi & Team  
**Last Updated**: February 22, 2026  
**Current Version**: 2.0.0 (Modernization in Progress)

## 🚦 Quick Status Dashboard

| Metric | Status |
|--------|--------|
| Build Status | ✅ Passing |
| Test Coverage | 65% |
| Security Scan | ✅ Passed |
| Performance | ⚡ Good |
| Documentation | 📚 Updated |
| Active Contributors | 3 |

---

<div align="center">
    <sub>Built with ❤️ using .NET 8 and Azure</sub>
</div>
```