using System.Collections.Generic;
using System.Threading.Tasks;
using PricingService.Domain;

namespace PricingService.Data;

public interface IDataStore
{
    Dictionary<string, Tariff> Tariffs { get; }
    Task<Tariff> GetTariffAsync(string code);
}