using System.Collections.Generic;
using System.Linq;

namespace PricingService.Domain;

public class Tariff
{
    public string Code { get; }
    public BasePremiumRules BasePremiumRules { get; } = new();
    public DiscountMarkupRules DiscountMarkupRules { get; } = new();

    public Tariff(string code)
    {
        Code = code;
    }

    public PriceResult CalculatePrice(Calculation calculation)
    {
        var basePrices = BasePremiumRules.CalculateBasePrices(calculation);
        var result = new PriceResult();

        foreach (var kvp in basePrices)
        {
            var priceAfterMarkup = DiscountMarkupRules.ApplyMarkups(kvp.Value, calculation.Subject);
            result.Covers[kvp.Key] = new CoverPrice { Price = priceAfterMarkup };
        }

        result.TotalPremium = result.Covers.Values.Sum(c => c.Price);
        return result;
    }
}