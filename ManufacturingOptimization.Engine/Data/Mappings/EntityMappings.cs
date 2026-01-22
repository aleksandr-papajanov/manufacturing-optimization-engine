using AutoMapper;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Engine.Data.Mappings;

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

        // OptimizationPlan mappings
        CreateMap<OptimizationPlanEntity, OptimizationPlan>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OptimizationPlanStatus>(src.Status)))
            .ReverseMap()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SelectedStrategy, opt => opt.Ignore());

        CreateMap<OptimizationStrategy, OptimizationStrategyEntity>()
            .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps))
            .ForMember(dest => dest.Metrics, opt => opt.MapFrom(src => src.Metrics))
            .ForMember(dest => dest.Warranty, opt => opt.MapFrom(src => src.Warranty))
            .ForMember(dest => dest.Plan, opt => opt.Ignore());
        
        CreateMap<OptimizationStrategyEntity, OptimizationStrategy>();

        CreateMap<WarrantyTerms, WarrantyTermsEntity>()
            .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
            .ForMember(dest => dest.Strategy, opt => opt.Ignore());
        
        CreateMap<WarrantyTermsEntity, WarrantyTerms>();

        CreateMap<OptimizationProcessStep, ProcessStepEntity>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()))
            .ForMember(dest => dest.Estimate, opt => opt.MapFrom(src => src.Estimate))
            .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
            .ForMember(dest => dest.Strategy, opt => opt.Ignore());
        
        CreateMap<ProcessStepEntity, OptimizationProcessStep>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

        CreateMap<ProcessEstimate, ProcessEstimateEntity>()
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration.Ticks))
            .ForMember(dest => dest.ProcessStepId, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessStep, opt => opt.Ignore());
        
        CreateMap<ProcessEstimateEntity, ProcessEstimate>()
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => TimeSpan.FromTicks(src.Duration)));

        CreateMap<OptimizationMetrics, OptimizationMetricsEntity>()
            .ForMember(dest => dest.TotalTime, opt => opt.MapFrom(src => src.TotalDuration.Ticks))
            .ForMember(dest => dest.TotalEmissionsKgCO2, opt => opt.MapFrom(src => src.TotalEmissionsKgCO2))
            .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
            .ForMember(dest => dest.Strategy, opt => opt.Ignore());
        
        CreateMap<OptimizationMetricsEntity, OptimizationMetrics>()
            .ForMember(dest => dest.TotalDuration, opt => opt.MapFrom(src => TimeSpan.FromTicks(src.TotalTime)))
            .ForMember(dest => dest.TotalEmissionsKgCO2, opt => opt.MapFrom(src => src.TotalEmissionsKgCO2));
    }
}
