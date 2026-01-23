using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Services
{
    public class OptimizationService : IOptimizationService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOptimizationStrategyRepository _strategyRepository;
        private readonly IOptimizationPlanRepository _planRepository;
        private readonly IMapper _mapper;

        public OptimizationService(
            IMessagePublisher messagePublisher,
            IOptimizationStrategyRepository strategyRepository,
            IOptimizationPlanRepository planRepository,
            IMapper mapper)
        {
            _messagePublisher = messagePublisher;
            _strategyRepository = strategyRepository;
            _planRepository = planRepository;
            _mapper = mapper;
        }

        public Task<Guid> RequestOptimizationPlanAsync(OptimizationRequestDto request)
        {
            var requestModel = _mapper.Map<OptimizationRequestModel>(request);
            var command = new RequestOptimizationPlanCommand { Request = requestModel };

            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested, command);

            return Task.FromResult(requestModel.RequestId);
        }

        public Task SelectStrategyAsync(Guid requestId, Guid strategyId)
        {
            var command = new SelectStrategyCommand
            {
                RequestId = requestId,
                SelectedStrategyId = strategyId
            };
            var routingKey = $"{OptimizationRoutingKeys.StrategySelected}.{command.RequestId}";
            _messagePublisher.Publish(Exchanges.Optimization, routingKey, command);
            return Task.CompletedTask;
        }

        public async Task<List<OptimizationStrategyDto>> GetStrategiesAsync(Guid requestId)
        {
            var strategies = await _strategyRepository.GetForRequestAsync(requestId);
            if (strategies == null || !strategies.Any())
                throw new NotFoundException($"No strategies found for request {requestId}");

            var strategyModels = _mapper.Map<List<OptimizationStrategyModel>>(strategies);
            var strategyDtos = _mapper.Map<List<OptimizationStrategyDto>>(strategyModels);
            return strategyDtos;
        }

        public async Task<OptimizationPlanDto> GetPlanAsync(Guid requestId)
        {
            var planEntity = await _planRepository.GetByRequestIdAsync(requestId);
            if (planEntity == null)
                throw new NotFoundException($"No optimization plan found for request {requestId}");

            var planModel = _mapper.Map<OptimizationPlanModel>(planEntity);
            var planDto = _mapper.Map<OptimizationPlanDto>(planModel);
            return planDto;
        }
    }
}
