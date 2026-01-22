using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Data.Mappings
{
    public class ProviderMappingProfile : Profile
    {
        public ProviderMappingProfile()
        {
            CreateMap<ProviderModel, ProviderEntity>()
                .ForMember(dest => dest.ProcessCapabilities, opt => opt.MapFrom(src => src.ProcessCapabilities))
                .ForMember(dest => dest.TechnicalCapabilities, opt => opt.MapFrom(src => src.TechnicalCapabilities));

            CreateMap<ProviderEntity, ProviderModel>();

            CreateMap<ProcessCapabilityModel, ProcessCapabilityEntity>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()))
                .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
                .ForMember(dest => dest.Provider, opt => opt.Ignore());

            CreateMap<ProcessCapabilityEntity, ProcessCapabilityModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

            CreateMap<TechnicalCapabilitiesModel, TechnicalCapabilitiesEntity>()
                .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
                .ForMember(dest => dest.Provider, opt => opt.Ignore());

            CreateMap<TechnicalCapabilitiesEntity, TechnicalCapabilitiesModel>();
        }
    }
}
