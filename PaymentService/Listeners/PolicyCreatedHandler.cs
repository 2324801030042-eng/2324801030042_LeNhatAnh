using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using PaymentService.Domain;
using PolicyService.Api.Events;

namespace PaymentService.Listeners;

public class PolicyCreatedHandler : INotificationHandler<PolicyCreated>
{
    private readonly IDocumentStore documentStore;
    private readonly PolicyAccountNumberGenerator policyAccountNumberGenerator;

    public PolicyCreatedHandler(
        IDocumentStore documentStore,
        PolicyAccountNumberGenerator policyAccountNumberGenerator)
    {
        this.documentStore = documentStore;
        this.policyAccountNumberGenerator = policyAccountNumberGenerator;
    }

    public async Task Handle(PolicyCreated notification, CancellationToken cancellationToken)
    {
        // Open a dedicated session per event to avoid disposing a shared scoped session
        // (fix ObjectDisposedException: 'This session has been disposed')
        await using (var session = documentStore.LightweightSession())
        {
            var policy = new PolicyAccount
            (
                notification.PolicyNumber,
                policyAccountNumberGenerator.Generate(),
                notification.PolicyHolder.FirstName,
                notification.PolicyHolder.LastName
            );

            if (await session.Query<PolicyAccount>()
                    .AnyAsync(p => p.PolicyNumber == notification.PolicyNumber, cancellationToken))
                return;

            session.Insert(policy);

            await session.SaveChangesAsync(cancellationToken);
        }
    }
}
