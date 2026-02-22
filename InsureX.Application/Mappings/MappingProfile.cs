using AutoMapper;
using InsureX.Domain.Entities;
using InsureX.Application.DTOs;

namespace InsureX.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Existing mappings...
        
        // Compliance mappings
        CreateMap<ComplianceResult, ComplianceCheckResultDto>()
            .ForMember(dest => dest.ActiveAlerts, 
                opt => opt.MapFrom(src => src.Alerts ?? new List<ComplianceAlert>()))
            .ForMember(dest => dest.Severity, 
                opt => opt.MapFrom(src => MapSeverity(src)));
        
        CreateMap<ComplianceAlert, ComplianceAlertDto>();
    }
    
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
}using AutoMapper;
using InsureX.Application.DTOs;
using InsureX.Domain.Entities;

namespace InsureX.Application.Common.Mappings;

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
    }
}