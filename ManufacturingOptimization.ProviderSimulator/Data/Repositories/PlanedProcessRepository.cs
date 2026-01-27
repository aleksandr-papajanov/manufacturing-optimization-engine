using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Data.Repositories;

public class PlanedProcessRepository : Repository<PlannedProcessEntity>, IPlannedProcessRepository
{
    public PlanedProcessRepository(IProviderSimulatorDbContext context) : base(context)
    {
    }
}
