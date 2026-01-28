using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Data.Repositories;

public class ProposalRepository : Repository<ProposalEntity>, IProposalRepository
{
    public ProposalRepository(IProviderSimulatorDbContext context) : base(context)
    {
    }
}
