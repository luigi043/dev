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

        public async Task<PagedResult<PolicyDto>> GetPagedAsync(PolicySearchDto search)
        {
            try
            {
                var query = await _policyRepository.GetQueryableAsync();
                
                // Apply tenant filter
                var tenantId = _tenantContext.GetCurrentTenantId();
                if (tenantId.HasValue)
                {
                    query = query.Where(p => p.TenantId == tenantId.Value);
                }

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(search.SearchTerm))
                {
                    query = query.Where(p => 
                        p.PolicyNumber.Contains(search.SearchTerm) ||
                        p.InsurerName.Contains(search.SearchTerm));
                }

                if (search.AssetId.HasValue)
                {
                    query = query.Where(p => p.AssetId == search.AssetId.Value);
                }

                if (!string.IsNullOrWhiteSpace(search.Status))
                {
                    query = query.Where(p => p.Status == search.Status);
                }

                if (search.ExpiringOnly == true)
                {
                    var expiryDate = DateTime.UtcNow.AddDays(30);
                    query = query.Where(p => p.EndDate <= expiryDate && 
                                           p.EndDate >= DateTime.UtcNow &&
                                           p.Status == "Active");
                }

                // Apply sorting
                query = ApplySorting(query, search.SortBy, search.SortDir);

                var totalItems = await _policyRepository.CountAsync(query);
                var items = await _policyRepository.GetPagedAsync(query, search.Page, search.PageSize);

                return new PagedResult<PolicyDto>
                {
                    Items = items.Adapt<List<PolicyDto>>(),
                    Page = search.Page,
                    PageSize = search.PageSize,
                    TotalItems = totalItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged policies");
                throw;
            }
        }

        public async Task<PolicyDto?> GetByIdAsync(int id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            return policy?.Adapt<PolicyDto>();
        }

        public async Task<PolicyDto> CreateAsync(CreatePolicyDto dto)
        {
            // Validate asset exists
            var asset = await _assetRepository.GetByIdAsync(dto.AssetId);
            if (asset == null)
            {
                throw new DomainException($"Asset with ID {dto.AssetId} not found");
            }

            // Check for duplicate policy number
            if (await _policyRepository.ExistsAsync(dto.PolicyNumber))
            {
                throw new DomainException($"Policy number {dto.PolicyNumber} already exists");
            }

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                throw new DomainException("End date must be after start date");
            }

            var policy = dto.Adapt<Policy>();
            policy.TenantId = _tenantContext.GetCurrentTenantId() 
                ?? throw new UnauthorizedAccessException("No tenant context");
            policy.CreatedBy = _currentUserService.GetCurrentUserId() ?? "system";
            policy.CreatedAt = DateTime.UtcNow;
            policy.Status = dto.StartDate <= DateTime.UtcNow ? "Active" : "Pending";
            policy.PaymentStatus = "Pending";

            await _policyRepository.AddAsync(policy);
            await _policyRepository.SaveChangesAsync();

            // Update asset compliance status
            await UpdateAssetCompliance(asset.Id);

            _logger.LogInformation("Policy created: {PolicyNumber} for Asset {AssetId}", 
                policy.PolicyNumber, policy.AssetId);

            // Send notification if needed
            if (policy.Status == "Active")
            {
                await _notificationService.SendEmailAsync(
                    _currentUserService.GetCurrentUserEmail() ?? "",
                    "Policy Created Successfully",
                    $"Policy {policy.PolicyNumber} has been created and is now active."
                );
            }

            return policy.Adapt<PolicyDto>();
        }

        public async Task<PolicyDto?> UpdateAsync(UpdatePolicyDto dto)
        {
            var policy = await _policyRepository.GetByIdAsync(dto.Id);
            if (policy == null)
            {
                throw new DomainException($"Policy with id {dto.Id} not found");
            }

            // Check if policy number changed and if it's unique
            if (policy.PolicyNumber != dto.PolicyNumber && 
                await _policyRepository.ExistsAsync(dto.PolicyNumber))
            {
                throw new DomainException($"Policy number {dto.PolicyNumber} already exists");
            }

            // Validate dates
            if (dto.EndDate <= dto.StartDate)
            {
                throw new DomainException("End date must be after start date");
            }

            // Store old status for notification
            var oldStatus = policy.Status;

            dto.Adapt(policy);
            policy.UpdatedAt = DateTime.UtcNow;
            policy.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";

            await _policyRepository.UpdateAsync(policy);
            await _policyRepository.SaveChangesAsync();

            // Update asset compliance
            await UpdateAssetCompliance(policy.AssetId);

            // Send notification if status changed
            if (oldStatus != policy.Status)
            {
                await _notificationService.SendEmailAsync(
                    _currentUserService.GetCurrentUserEmail() ?? "",
                    $"Policy Status Changed: {policy.PolicyNumber}",
                    $"Policy status has been updated from {oldStatus} to {policy.Status}"
                );
            }

            _logger.LogInformation("Policy updated: {PolicyNumber}", policy.PolicyNumber);

            return policy.Adapt<PolicyDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var policy = await _policyRepository.GetByIdAsync(id);
            if (policy == null)
            {
                return false;
            }

            var assetId = policy.AssetId;

            // Soft delete
            policy.Status = "Deleted";
            policy.UpdatedAt = DateTime.UtcNow;
            policy.UpdatedBy = _currentUserService.GetCurrentUserId() ?? "system";
            
            await _policyRepository.UpdateAsync(policy);
            await _policyRepository.SaveChangesAsync();

            // Update asset compliance
            await UpdateAssetCompliance(assetId);

            _logger.LogInformation("Policy deleted: {PolicyNumber}", policy.PolicyNumber);

            return true;
        }

        public async Task<List<PolicyDto>> GetExpiringPoliciesAsync(int days)
        {
            var policies = await _policyRepository.GetExpiringPoliciesAsync(days);
            return policies.Adapt<List<PolicyDto>>();
        }

        public async Task<List<PolicyDto>> GetByAssetIdAsync(int assetId)
        {
            var policies = await _policyRepository.GetByAssetIdAsync(assetId);
            return policies.Adapt<List<PolicyDto>>();
        }

        public async Task<PolicySummaryDto> GetSummaryAsync()
        {
            var tenantId = _tenantContext.GetCurrentTenantId();
            var query = await _policyRepository.GetQueryableAsync();
            
            if (tenantId.HasValue)
            {
                query = query.Where(p => p.TenantId == tenantId.Value);
            }

            var policies = await Task.FromResult(query.ToList());
            var now = DateTime.UtcNow;

            return new PolicySummaryDto
            {
                TotalPolicies = policies.Count,
                ActivePolicies = policies.Count(p => p.Status == "Active" && p.EndDate >= now),
                ExpiringPolicies = policies.Count(p => p.Status == "Active" && 
                    p.EndDate >= now && p.EndDate <= now.AddDays(30)),
                ExpiredPolicies = policies.Count(p => p.EndDate < now),
                TotalSumInsured = policies.Sum(p => p.SumInsured),
                TotalPremium = policies.Sum(p => p.Premium),
                PoliciesByType = policies
                    .GroupBy(p => p.PolicyType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                PoliciesByInsurer = policies
                    .GroupBy(p => p.InsurerName)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public async Task<bool> CheckComplianceAsync(int assetId)
        {
            var policies = await _policyRepository.GetByAssetIdAsync(assetId);
            
            // Asset is compliant if it has at least one active policy
            var hasActivePolicy = policies.Any(p => 
                p.Status == "Active" && 
                p.EndDate >= DateTime.UtcNow &&
                p.PaymentStatus == "Paid");

            return hasActivePolicy;
        }

        public async Task<ClaimDto?> GetClaimByIdAsync(int claimId)
        {
            var claim = await _policyRepository.GetClaimByIdAsync(claimId);
            return claim?.Adapt<ClaimDto>();
        }

        public async Task<List<ClaimDto>> GetClaimsByPolicyIdAsync(int policyId)
        {
            var claims = await _policyRepository.GetClaimsByPolicyIdAsync(policyId);
            return claims.Adapt<List<ClaimDto>>();
        }

        public async Task<ClaimDto> AddClaimAsync(CreateClaimDto dto)
        {
            var policy = await _policyRepository.GetByIdAsync(dto.PolicyId);
            if (policy == null)
            {
                throw new DomainException($"Policy with ID {dto.PolicyId} not found");
            }

            var claim = dto.Adapt<PolicyClaim>();
            claim.Status = "Submitted";

            await _policyRepository.AddClaimAsync(claim);
            await _policyRepository.SaveChangesAsync();

            // Update policy claim count
            policy.ClaimsCount = (policy.ClaimsCount ?? 0) + 1;
            policy.LastClaimDate = dto.ClaimDate;
            await _policyRepository.UpdateAsync(policy);
            await _policyRepository.SaveChangesAsync();

            _logger.LogInformation("Claim submitted for Policy {PolicyNumber}, Amount: {Amount}", 
                policy.PolicyNumber, dto.ClaimAmount);

            // Send notification
            await _notificationService.SendEmailAsync(
                _currentUserService.GetCurrentUserEmail() ?? "",
                "Claim Submitted Successfully",
                $"Your claim for policy {policy.PolicyNumber} has been submitted. Amount: {dto.ClaimAmount:C}"
            );

            return claim.Adapt<ClaimDto>();
        }

        public async Task<ClaimDto?> UpdateClaimStatusAsync(int claimId, string status, decimal? settlementAmount)
        {
            var claim = await _policyRepository.GetClaimByIdAsync(claimId);
            if (claim == null)
            {
                return null;
            }

            claim.Status = status;
            
            if (status == "Settled" && settlementAmount.HasValue)
            {
                claim.SettlementDate = DateTime.UtcNow;
                claim.SettlementAmount = settlementAmount.Value;
            }

            await _policyRepository.UpdateClaimAsync(claim);
            await _policyRepository.SaveChangesAsync();

            _logger.LogInformation("Claim {ClaimId} status updated to {Status}", claimId, status);

            return claim.Adapt<ClaimDto>();
        }

        public async Task<int> UpdateExpiredPoliciesAsync()
        {
            var expiredPolicies = await _policyRepository.GetExpiredPoliciesAsync();
            
            foreach (var policy in expiredPolicies)
            {
                policy.Status = "Expired";
                policy.UpdatedAt = DateTime.UtcNow;
                policy.UpdatedBy = "system";
                await _policyRepository.UpdateAsync(policy);
                
                // Update asset compliance
                await UpdateAssetCompliance(policy.AssetId);
            }

            await _policyRepository.SaveChangesAsync();

            _logger.LogInformation("Updated {Count} expired policies", expiredPolicies.Count);

            return expiredPolicies.Count;
        }

        public async Task<int> SendRenewalRemindersAsync(int daysBeforeExpiry)
        {
            var expiringPolicies = await _policyRepository.GetExpiringPoliciesAsync(daysBeforeExpiry);
            
            foreach (var policy in expiringPolicies)
            {
                await _notificationService.SendEmailAsync(
                    _currentUserService.GetCurrentUserEmail() ?? "",
                    $"Policy Renewal Reminder: {policy.PolicyNumber}",
                    $"Your policy for asset {policy.Asset?.AssetTag} will expire on {policy.EndDate:d}. " +
                    $"Please arrange renewal."
                );
            }

            return expiringPolicies.Count;
        }

        // ======= Private Helper Methods =======

        private IQueryable<Policy> ApplySorting(IQueryable<Policy> query, string? sortBy, string sortDir)
        {
            var isAscending = sortDir?.ToLower() == "asc";

            return sortBy?.ToLower() switch
            {
                "policynumber" => isAscending 
                    ? query.OrderBy(p => p.PolicyNumber) 
                    : query.OrderByDescending(p => p.PolicyNumber),
                "insurername" => isAscending 
                    ? query.OrderBy(p => p.InsurerName) 
                    : query.OrderByDescending(p => p.InsurerName),
                "startdate" => isAscending 
                    ? query.OrderBy(p => p.StartDate) 
                    : query.OrderByDescending(p => p.StartDate),
                "enddate" => isAscending 
                    ? query.OrderBy(p => p.EndDate) 
                    : query.OrderByDescending(p => p.EndDate),
                "suminsured" => isAscending 
                    ? query.OrderBy(p => p.SumInsured) 
                    : query.OrderByDescending(p => p.SumInsured),
                "status" => isAscending 
                    ? query.OrderBy(p => p.Status) 
                    : query.OrderByDescending(p => p.Status),
                _ => isAscending 
                    ? query.OrderBy(p => p.EndDate) 
                    : query.OrderByDescending(p => p.EndDate)
            };
        }

        private async Task UpdateAssetCompliance(int assetId)
        {
            var asset = await _assetRepository.GetByIdAsync(assetId);
            if (asset != null)
            {
                var isCompliant = await CheckComplianceAsync(assetId);
                asset.ComplianceStatus = isCompliant ? "Compliant" : "Non-Compliant";
                asset.UpdatedAt = DateTime.UtcNow;
                asset.UpdatedBy = "system";
                await _assetRepository.UpdateAsync(asset);
                await _assetRepository.SaveChangesAsync();
            }
        }
    }
}