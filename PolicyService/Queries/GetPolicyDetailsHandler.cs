using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using PolicyService.Api.Queries;
using PolicyService.Api.Queries.Dto;
using PolicyService.Domain;
using PolicyService.RestClients;

namespace PolicyService.Queries;

public class GetPolicyDetailsHandler : IRequestHandler<GetPolicyDetailsQuery, GetPolicyDetailsQueryResult>
{
    private readonly IUnitOfWork uow;
    private readonly IPaymentClient paymentClient;
    private readonly ILogger<GetPolicyDetailsHandler> logger;

    public GetPolicyDetailsHandler(
        IUnitOfWork uow,
        IPaymentClient paymentClient,
        ILogger<GetPolicyDetailsHandler> logger)
    {
        this.uow = uow;
        this.paymentClient = paymentClient;
        this.logger = logger;
    }

    public async Task<GetPolicyDetailsQueryResult> Handle(GetPolicyDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var policy = await uow.Policies.WithNumber(request.PolicyNumber);
        if (policy == null) throw new ApplicationException($"Policy {request.PolicyNumber} not found!");

        return ConstructResult(policy);
    }

    private GetPolicyDetailsQueryResult ConstructResult(Policy policy)
    {
        var effectiveVersion = policy.Versions.FirstVersion();

        return new GetPolicyDetailsQueryResult
        {
            Policy = new PolicyDetailsDto
            {
                Number = policy.Number,
                ProductCode = policy.ProductCode,
                DateFrom = effectiveVersion.CoverPeriod.ValidFrom,
                DateTo = effectiveVersion.CoverPeriod.ValidTo,
                PolicyHolder = $"{effectiveVersion.PolicyHolder.FirstName} {effectiveVersion.PolicyHolder.LastName}",
                TotalPremium = effectiveVersion.TotalPremiumAmount,

                AccountNumber = TryGetAccountNumber(policy.Number),
                Covers = effectiveVersion.Covers.Select(c => c.Code).ToList()
            }
        };
    }

    private string TryGetAccountNumber(string policyNumber)
    {
        try
        {
            var result = paymentClient.GetAccountBalance(policyNumber).GetAwaiter().GetResult();
            return result?.Balance?.PolicyAccountNumber;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not fetch payment account for policy {PolicyNumber}", policyNumber);
            return null;
        }
    }
}
