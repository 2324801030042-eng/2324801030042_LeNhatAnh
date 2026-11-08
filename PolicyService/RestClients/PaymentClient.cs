using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RestEase;
using Steeltoe.Common.Discovery;
using Steeltoe.Discovery;

namespace PolicyService.RestClients;

public interface IPaymentClient
{
    [Get("accounts/{policyNumber}")]
    Task<PaymentAccountBalanceResponse> GetAccountBalance([Path("policyNumber")] string policyNumber);
}

public class PaymentAccountBalanceResponse
{
    [JsonProperty("balance")]
    public PaymentAccountBalance Balance { get; set; }
}

public class PaymentAccountBalance
{
    [JsonProperty("policyNumber")]
    public string PolicyNumber { get; set; }

    [JsonProperty("policyAccountNumber")]
    public string PolicyAccountNumber { get; set; }

    [JsonProperty("balance")]
    public decimal Balance { get; set; }
}

public class PaymentClient : IPaymentClient
{
    private static readonly AsyncRetryPolicy retryPolicy = Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(2));

    private readonly IPaymentClient client;

    public PaymentClient(IConfiguration configuration, IDiscoveryClient discoveryClient)
    {
        var handler = new DiscoveryHttpClientHandler(discoveryClient);
        var httpClient = new HttpClient(handler, false)
        {
            BaseAddress = new Uri(configuration.GetValue<string>("PaymentServiceUri"))
        };
        client = RestClient.For<IPaymentClient>(httpClient);
    }

    public Task<PaymentAccountBalanceResponse> GetAccountBalance(string policyNumber)
    {
        return retryPolicy.ExecuteAsync(async () =>
            await client.GetAccountBalance(Uri.EscapeDataString(policyNumber)));
    }
}
