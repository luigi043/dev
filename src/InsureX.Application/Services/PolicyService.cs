using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InsureX.Application.Interfaces;
using InsureX.Application.DTOs;
using InsureX.Domain.Entities;
using InsureX.Domain.Exceptions;
using InsureX.Domain.Interfaces;
using Mapster;

namespace InsureX.Application.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly ITenantContext _tenantContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PolicyService> _logger;

        public PolicyService(
            IPolicyRepository policyRepository,
            IAssetRepository assetRepository,
            ITenantContext tenantContext,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<PolicyService> logger)
        {
            _policyRepository = policyRepository;
            _assetRepository = assetRepository;
            _tenantContext = tenantContext;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<PolicyDto>> GetAllAsync()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            var query = await _policyRepository.GetQueryableAsync();
            var policies = query.Where(p => p.TenantId == tenantId).ToList();
            return policies.Adapt<List<PolicyDto>>();
        }

        public async Task<PolicyDto?> GetByIdAsync(int id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            return policy?.Adapt<PolicyDto>();
        }

        public async Task<PolicyDto> CreateAsync(CreatePolicyDto dto)
        {
            var asset = await _assetRepository.GetByIdAsync(dto.AssetId)
                        ?? throw new DomainException($"Asset {dto.AssetId} not found");

            if (await _policyRepository.ExistsAsync(dto.PolicyNumber))
                throw new DomainException($"Policy {dto.PolicyNumber} already exists");

            var policy = dto.Adapt<Policy>();
            policy.TenantId = _tenantContext.GetCurrentTenantId() ?? throw new UnauthorizedAccessException();
            policy.CreatedBy = _currentUserService.GetCurrentUserId() ?? "system";
            policy.CreatedAt = DateTime.UtcNow;
            policy.Status = dto.StartDate <= DateTime.UtcNow ? "Active" : "Pending";
            policy.PaymentStatus = "Pending";

            await _policyRepository.AddAsync(policy);
            await _policyRepository.SaveChangesAsync();

            // Update compliance status
            await UpdateAssetCompliance(asset.Id);

            // Notify user if active
            if (policy.Status == "Active")
            {
                await _notificationService.SendEmailAsync(
                    _currentUserService.GetCurrentUserEmail() ?? "",
                    "Policy Created",
                    $"Policy {policy.PolicyNumber} is now active."
                );
            }

            _logger.LogInformation("Policy created: {PolicyNumber}", policy.PolicyNumber);
            return policy.Adapt<PolicyDto>();
        }

        public async Task<PolicyDto?> UpdateAsync(UpdatePolicyDto dto)
        {
            var policy = await _policyRepository.GetByIdAsync(dto.Id)
                         ?? throw new DomainException($"Policy {dto.Id} not found");

            dto.Adapt(policy);
            policy.UpdatedAt = DateTime.UtcNow;
            policy.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";

            await _policyRepository.UpdateAsync(policy);
            await _policyRepository.SaveChangesAsync();

            // Update compliance
            await UpdateAssetCompliance(policy.AssetId);

            _logger.LogInformation("Policy updated: {PolicyNumber}", policy.PolicyNumber);
            return policy.Adapt<PolicyDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            if (policy == null) return false;

            policy.Status = "Deleted";
            policy.UpdatedAt = DateTime.UtcNow;
            policy.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";

            await _policyRepository.UpdateAsync(policy);
            await _policyRepository.SaveChangesAsync();

            await UpdateAssetCompliance(policy.AssetId);

            _logger.LogInformation("Policy deleted: {PolicyNumber}", policy.PolicyNumber);
            return true;
        }

        public async Task<List<PolicyDto>> GetExpiringPoliciesAsync(int days)
        {
            var policies = await _policyRepository.GetExpiringPoliciesAsync(days);
            return policies.Adapt<List<PolicyDto>>();
        }

        // ======= Private helper =======
        private async Task UpdateAssetCompliance(int assetId)
        {
            var asset = await _assetRepository.GetByIdAsync(assetId);
            if (asset == null) return;

            asset.ComplianceStatus = await CheckComplianceAsync(assetId) ? "Compliant" : "Non-Compliant";
            asset.UpdatedAt = DateTime.UtcNow;
            asset.UpdatedBy = "system";

            await _assetRepository.UpdateAsync(asset);
            await _assetRepository.SaveChangesAsync();
        }

        private async Task<bool> CheckComplianceAsync(int assetId)
        {
            var policies = await _policyRepository.GetByAssetIdAsync(assetId);
            return policies.Any(p => p.Status == "Active" && p.EndDate >= DateTime.UtcNow && p.PaymentStatus == "Paid");
        }
    }
}