using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Data.Mappings;

public class ProcessEstimateMappingProfile : Profile
{
    public ProcessEstimateMappingProfile()
    {
        CreateMap<ProcessEstimateModel, ProcessEstimateEntity>()
            .ForMember(dest => dest.DurationTicks, opt => opt.MapFrom(src => src.Duration.Ticks))
            .ForMember(dest => dest.ProposalId, opt => opt.Ignore());

        CreateMap<ProcessEstimateEntity, ProcessEstimateModel>()
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => TimeSpan.FromTicks(src.DurationTicks)));
    }
}
