using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Data.Mappings;

public class ProposalMappingProfile : Profile
{
    public ProposalMappingProfile()
    {
        CreateMap<ProposalModel, ProposalEntity>()
            .ForMember(dest => dest.EstimateId, opt => opt.Ignore())
            .ForMember(dest => dest.PlannedProcessId, opt => opt.Ignore());

        CreateMap<ProposalEntity, ProposalModel>();
    }
}
