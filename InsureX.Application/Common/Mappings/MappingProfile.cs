using AutoMapper;
using InsureX.Domain.Entities;
using InsureX.Application.DTOs;

namespace InsureX.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Asset mappings
        CreateMap<Asset, AssetDto>();
        CreateMap<CreateAssetDto, Asset>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.Policies, opt => opt.Ignore())
            .ForMember(dest => dest.ComplianceResults, opt => opt.Ignore());

        CreateMap<UpdateAssetDto, Asset>()
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.Policies, opt => opt.Ignore())
            .ForMember(dest => dest.ComplianceResults, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Policy mappings
        CreateMap<Policy, PolicyDto>()
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.AssetTag : string.Empty));
        CreateMap<CreatePolicyDto, Policy>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Asset, opt => opt.Ignore())
            .ForMember(dest => dest.Claims, opt => opt.Ignore());
        CreateMap<UpdatePolicyDto, Policy>()
            .IncludeBase<CreatePolicyDto, Policy>();

        // Claim mappings
        CreateMap<PolicyClaim, ClaimDto>();
        CreateMap<CreateClaimDto, PolicyClaim>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Policy, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Submitted"));

        // Compliance mappings
        CreateMap<ComplianceRule, ComplianceRuleDto>();
        CreateMap<CreateComplianceRuleDto, ComplianceRule>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<ComplianceAlert, ComplianceAlertDto>()
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.AssetTag : string.Empty));

        CreateMap<ComplianceCheck, ComplianceCheckResultDto>()
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.AssetTag : string.Empty))
            .ForMember(dest => dest.ActiveAlerts, opt => opt.Ignore())
            .ForMember(dest => dest.Violations, opt => opt.Ignore())
            .ForMember(dest => dest.Warnings, opt => opt.Ignore());

        CreateMap<ComplianceCheck, ComplianceCheckDto>()
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.AssetTag : string.Empty));

        CreateMap<ComplianceHistory, ComplianceHistoryDto>()
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.AssetTag : string.Empty));
    }
}