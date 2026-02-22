using AutoMapper;
using InsureX.Domain.Entities;
using InsureX.Application.DTOs;
using System;

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

        // Policy mappings (if needed)
        // CreateMap<Policy, PolicyDto>();
        // CreateMap<CreatePolicyDto, Policy>();
        // CreateMap<UpdatePolicyDto, Policy>();

        // Compliance mappings
        CreateMap<ComplianceAlert, ComplianceAlertDto>();
        
        // Note: ComplianceResult and ComplianceCheckResultDto need to be defined
        // Uncomment when these DTOs are created:
        /*
        CreateMap<ComplianceResult, ComplianceCheckResultDto>()
            .ForMember(dest => dest.ActiveAlerts, 
                opt => opt.MapFrom(src => src.Alerts ?? new List<ComplianceAlert>()))
            .ForMember(dest => dest.Severity, 
                opt => opt.MapFrom(src => MapSeverity(src)));
        */
    }
    
    // Helper method for compliance severity mapping
    private ComplianceSeverity MapSeverity(ComplianceResult result)
    {
        if (!result.IsCompliant)
        {
            return result.Status switch
            {
                "Critical" => ComplianceSeverity.Critical,
                "Warning" => ComplianceSeverity.Warning,
                _ => ComplianceSeverity.Minor
            };
        }
        
        return result.NextCheckDue < DateTime.UtcNow.AddDays(-7) 
            ? ComplianceSeverity.Warning 
            : ComplianceSeverity.Info;
    }
}

// Define missing enums if needed
public enum ComplianceSeverity
{
    Info,
    Minor,
    Warning,
    Critical
}