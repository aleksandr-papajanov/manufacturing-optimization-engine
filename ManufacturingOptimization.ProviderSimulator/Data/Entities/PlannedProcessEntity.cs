using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.ProviderSimulator.Data.Entities
{
    public class PlannedProcessEntity
    {
        public Guid Id { get; set; }
        public Guid ProposalId { get; set; }
        public Guid AllocatedSlotId { get; set; }

        // Navigation properties
        public ProposalEntity Proposal { get; set; } = null!;
        public AllocatedSlotEntity AllocatedSlot { get; set; } = null!;
    }
}