using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderSimulator.Data.Repositories;

public class PlanedProcessRepository : Repository<PlannedProcessEntity>, IPlannedProcessRepository
{
    public PlanedProcessRepository(IProviderSimulatorDbContext context) : base(context)
    {
    }

    public async Task<List<PlannedProcessEntity>> GetAllInTimeWindowAsync(Guid providerId, DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Include(p => p.Proposal)
            .Include(p => p.AllocatedSlot)
                .ThenInclude(slot => slot.Segments)
            .Where(p => p.Proposal.ProviderId == providerId 
                && p.AllocatedSlot.StartTime < endTime 
                && p.AllocatedSlot.EndTime > startTime)
            .OrderBy(p => p.AllocatedSlot.StartTime)
            .ToListAsync();
    }
}
