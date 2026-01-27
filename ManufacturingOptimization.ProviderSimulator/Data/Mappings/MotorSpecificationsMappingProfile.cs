using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Data.Mappings;

public class MotorSpecificationsMappingProfile : Profile
{
    public MotorSpecificationsMappingProfile()
    {
        CreateMap<MotorSpecificationsModel, MotorSpecificationsEntity>()
            .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => src.CurrentEfficiency.ToString()))
            .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => src.TargetEfficiency.ToString()));

        CreateMap<MotorSpecificationsEntity, MotorSpecificationsModel>()
            .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.CurrentEfficiency)))
            .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.TargetEfficiency)));
    }
}
