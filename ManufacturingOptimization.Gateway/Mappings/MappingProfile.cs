using AutoMapper;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // OptimizationRequest mappings
            CreateMap<OptimizationRequest, OptimizationRequestDto>();
            CreateMap<OptimizationRequestDto, OptimizationRequest>();
            
            CreateMap<MotorSpecifications, MotorSpecificationsDto>()
                .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => src.CurrentEfficiency.ToString()))
                .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => src.TargetEfficiency.ToString()));
            CreateMap<MotorSpecificationsDto, MotorSpecifications>()
                .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.CurrentEfficiency)))
                .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.TargetEfficiency)));
            
            CreateMap<OptimizationRequestConstraints, OptimizationRequestConstraintsDto>();
            CreateMap<OptimizationRequestConstraintsDto, OptimizationRequestConstraints>();
            
            // OptimizationStrategy mappings
            CreateMap<OptimizationStrategy, OptimizationStrategyDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
            CreateMap<OptimizationStrategyDto, OptimizationStrategy>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<OptimizationPriority>(src.Priority)));
            
            CreateMap<OptimizationProcessStep, OptimizationProcessStepDto>()
                .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => src.Process.GetDisplayName()));
            CreateMap<OptimizationProcessStepDto, OptimizationProcessStep>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => ProcessTypeExtensions.Parse(src.Activity)));
            
            CreateMap<ProcessEstimate, ProcessEstimateDto>();
            CreateMap<ProcessEstimateDto, ProcessEstimate>();
            
            CreateMap<OptimizationMetrics, OptimizationMetricsDto>();
            CreateMap<OptimizationMetricsDto, OptimizationMetrics>();
            
            // Provider mappings
            CreateMap<Provider, ProviderDto>();
            CreateMap<ProviderDto, Provider>();
            
            CreateMap<ProviderProcessCapability, ProviderProcessCapabilityDto>();
            CreateMap<ProviderProcessCapabilityDto, ProviderProcessCapability>();
            
            CreateMap<ProviderTechnicalCapabilities, ProviderTechnicalCapabilitiesDto>();
            CreateMap<ProviderTechnicalCapabilitiesDto, ProviderTechnicalCapabilities>();
        }
    }
}
