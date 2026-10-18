using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using PricingService.Api.Commands;
using PricingService.Api.Commands.Dto;
using PricingService.Data;
using PricingService.Domain;

namespace PricingService.Commands;

public class CalculatePriceHandler : IRequestHandler<CalculatePriceCommand, CalculatePriceResult>
{
    private readonly IDataStore _dataStore;

    public CalculatePriceHandler(IDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public async Task<CalculatePriceResult> Handle(CalculatePriceCommand request, CancellationToken cancellationToken)
    {
        var validator = new CalculatePriceCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var tariff = await _dataStore.GetTariffAsync(request.ProductCode);
        
        if (tariff == null)
        {
            return CalculatePriceResult.Empty();
        }

        var subject = new Dictionary<string, object>();
        foreach (var answer in request.Answers)
        {
            subject[answer.QuestionCode] = answer.GetAnswer();
        }

        var calculation = new Calculation(
            request.ProductCode,
            request.PolicyFrom,
            request.PolicyTo,
            request.SelectedCovers,
            subject);

        var priceResult = tariff.CalculatePrice(calculation);

        return new CalculatePriceResult
        {
            TotalPrice = priceResult.TotalPremium,
            CoverPrices = priceResult.Covers.ToDictionary(k => k.Key, v => v.Value.Price)
        };
    }
}