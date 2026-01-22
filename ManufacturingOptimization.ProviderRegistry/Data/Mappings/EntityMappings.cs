using AutoMapper;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.ProviderRegistry.Data.Mappings;

/// <summary>
/// AutoMapper profile for mapping between Entity classes and domain models.
/// </summary>
public class EntityMappingProfile : Profile
{
    public EntityMappingProfile()
    {
        // Provider mappings
        CreateMap<Provider, ProviderEntity>()
            .ForMember(dest => dest.ProcessCapabilities, opt => opt.MapFrom(src => src.ProcessCapabilities))
            .ForMember(dest => dest.TechnicalCapabilities, opt => opt.MapFrom(src => src.TechnicalCapabilities));
        
        CreateMap<ProviderEntity, Provider>();

        CreateMap<ProviderProcessCapability, ProcessCapabilityEntity>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()))
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore());
        
        CreateMap<ProcessCapabilityEntity, ProviderProcessCapability>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

        CreateMap<ProviderTechnicalCapabilities, TechnicalCapabilitiesEntity>()
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore());
        
        CreateMap<TechnicalCapabilitiesEntity, ProviderTechnicalCapabilities>();
    }
}
