namespace ManufacturingOptimization.Common.Messaging.Abstractions
{
    public interface ICommand
    {
        public Guid CommandId { get; set; }
    }
}
