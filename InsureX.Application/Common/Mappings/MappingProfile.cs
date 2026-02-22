using AutoMapper;
using InsureX.Application.DTOs;
using InsureX.Domain.Entities;

namespace InsureX.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Asset, AssetDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        
        CreateMap<CreateAssetDto, Asset>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AssetStatus.Active))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.ComplianceResults, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        CreateMap<UpdateAssetDto, Asset>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<AssetStatus>(src.Status)))
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Tenant, opt => opt.Ignore())
            .ForMember(dest => dest.ComplianceResults, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        CreateMap<ComplianceResult, ComplianceResultDto>()
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.AssetTag : string.Empty))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()));
    }
}