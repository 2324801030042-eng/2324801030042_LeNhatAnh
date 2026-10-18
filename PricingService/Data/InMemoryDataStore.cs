using System.Collections.Generic;
using System.Threading.Tasks;
using PricingService.Domain;

namespace PricingService.Data;

public class InMemoryDataStore : IDataStore
{
    public Dictionary<string, Tariff> Tariffs { get; } = new();

    public Task<Tariff> GetTariffAsync(string code)
    {
        Tariffs.TryGetValue(code, out var tariff);
        return Task.FromResult(tariff);
    }
}