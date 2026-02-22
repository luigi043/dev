using AutoMapper;
using InsureX.Application.Common.Interfaces;
using InsureX.Application.Common.Models;
using InsureX.Application.DTOs;
using InsureX.Application.Interfaces;
using InsureX.Domain.Entities;
using InsureX.Domain.Interfaces;

namespace InsureX.Application.Services;

public class ComplianceService : IComplianceService
{
    private readonly IRepository<ComplianceResult> _complianceRepository;
    private readonly IRepository<Asset> _assetRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;

    public ComplianceService(
        IRepository<ComplianceResult> complianceRepository,
        IRepository<Asset> assetRepository,
        ITenantContext tenantContext,
        IMapper mapper)
    {
        _complianceRepository = complianceRepository;
        _assetRepository = assetRepository;
        _tenantContext = tenantContext;
        _mapper = mapper;
    }

    public async Task<ComplianceResultDto?> CheckAssetComplianceAsync(Guid assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null || asset.TenantId != _tenantContext.TenantId)
            return null;

        // Simple compliance logic - can be expanded
        var isCompliant = asset.Status == AssetStatus.Active;
        var severity = isCompliant ? ComplianceSeverity.Info : ComplianceSeverity.Critical;

        var result = new ComplianceResult
        {
            AssetId = assetId,
            IsCompliant = isCompliant,
            Status = isCompliant ? "Compliant" : "Non-Compliant",
            Message = isCompliant ? "Asset is compliant" : "Asset is not active",
            CheckedAt = DateTime.UtcNow,
            Severity = severity,
            Details = $"{{ \"assetTag\": \"{asset.AssetTag}\", \"status\": \"{asset.Status}\" }}"
        };

        var saved = await _complianceRepository.AddAsync(result);
        return _mapper.Map<ComplianceResultDto>(saved);
    }

    public async Task<PagedResult<ComplianceResultDto>> GetComplianceHistoryAsync(Guid assetId, int page, int pageSize)
    {
        var results = await _complianceRepository.FindAsync(c => c.AssetId == assetId);
        
        var totalItems = results.Count();
        var items = results
            .OrderByDescending(c => c.CheckedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<ComplianceResultDto>
        {
            Items = _mapper.Map<List<ComplianceResultDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            HasNext = page * pageSize < totalItems
        };
    }

    public async Task<double> GetComplianceRateAsync()
    {
        var assets = await _assetRepository.FindAsync(a => a.TenantId == _tenantContext.TenantId);
        if (!assets.Any()) return 100;

        var compliantCount = assets.Count(a => a.Status == AssetStatus.Active);
        return Math.Round((double)compliantCount / assets.Count() * 100, 2);
    }

    public async Task<List<ComplianceResultDto>> GetRecentAlertsAsync(int count)
    {
        var alerts = await _complianceRepository.FindAsync(c => 
            !c.IsCompliant && c.Severity == ComplianceSeverity.Critical);
        
        return _mapper.Map<List<ComplianceResultDto>>(
            alerts.OrderByDescending(c => c.CheckedAt).Take(count).ToList()
        );
    }
}