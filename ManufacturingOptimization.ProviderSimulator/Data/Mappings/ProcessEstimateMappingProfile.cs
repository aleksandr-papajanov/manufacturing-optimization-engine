using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using System.Text.Json;

namespace ManufacturingOptimization.ProviderSimulator.Data.Mappings;

public class ProcessEstimateMappingProfile : Profile
{
    public ProcessEstimateMappingProfile()
    {
        CreateMap<ProcessEstimateModel, ProcessEstimateEntity>()
            .ForMember(dest => dest.ProposalId, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableTimeSlotsJson, opt => opt.MapFrom(src =>
                    src.AvailableTimeSlots != null && src.AvailableTimeSlots.Any()
                        ? JsonSerializer.Serialize(src.AvailableTimeSlots, new JsonSerializerOptions { WriteIndented = false })
                        : null));

        CreateMap<ProcessEstimateEntity, ProcessEstimateModel>()
            .ForMember(dest => dest.AvailableTimeSlots, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.AvailableTimeSlotsJson)
                    ? JsonSerializer.Deserialize<List<TimeWindowModel>>(src.AvailableTimeSlotsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TimeWindowModel>()
                    : new List<TimeWindowModel>()));
            
    }
}
