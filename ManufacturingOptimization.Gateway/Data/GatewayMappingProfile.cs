using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Gateway.Data
{
    public class GatewayMappingProfile : Profile
    {
        public GatewayMappingProfile()
        {
            CreateMap<OptimizationRequestModel, OptimizationRequestDto>().ReverseMap();

            CreateMap<OptimizationRequestConstraintsModel, OptimizationRequestConstraintsDto>().ReverseMap();

            CreateMap<MotorSpecificationsModel, MotorSpecificationsDto>().ReverseMap();

            CreateMap<OptimizationPlanModel, OptimizationPlanDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<OptimizationPlanDto, OptimizationPlanModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OptimizationPlanStatus>(src.Status)));

            CreateMap<OptimizationStrategyModel, OptimizationStrategyDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
            CreateMap<OptimizationStrategyDto, OptimizationStrategyModel>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<OptimizationPriority>(src.Priority)));

            CreateMap<OptimizationMetricsModel, OptimizationMetricsDto>().ReverseMap();

            CreateMap<ProcessStepModel, ProcessStepDto>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()));
            CreateMap<ProcessStepDto, ProcessStepModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

            CreateMap<ProcessEstimateModel, ProcessEstimateDto>().ReverseMap();

            CreateMap<ProcessCapabilityModel, ProcessCapabilityDto>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()));
            CreateMap<ProcessCapabilityDto, ProcessCapabilityModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

            CreateMap<ProviderModel, ProviderDto>().ReverseMap();

            CreateMap<TechnicalCapabilitiesModel, TechnicalCapabilitiesDto>().ReverseMap();

            CreateMap<WarrantyTermsModel, WarrantyTermsDto>().ReverseMap();
        }
    }
}
