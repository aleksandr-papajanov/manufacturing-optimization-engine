using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.ProviderSimulator.Data.Entities
{
    public class PlannedProcessEntity
    {
        public Guid Id { get; set; }
        public Guid ProposalId { get; set; }

        // Navigation property
        public ProposalEntity Proposal { get; set; } = null!;
    }
}