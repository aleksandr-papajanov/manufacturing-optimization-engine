using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IMapper _mapper;

        public ProviderService(IProviderRepository providerRepository, IMapper mapper)
        {
            _providerRepository = providerRepository;
            _mapper = mapper;
        }

        public async Task<List<ProviderDto>> GetProvidersAsync()
        {
            var providers = await _providerRepository.GetAllAsync();
            var providerList = providers.ToList();
            var providerModels = _mapper.Map<List<ProviderModel>>(providerList);
            var providerDtos = _mapper.Map<List<ProviderDto>>(providerModels);
            return providerDtos;
        }
    }
}
