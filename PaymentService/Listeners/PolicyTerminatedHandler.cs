using System.Threading;
using System.Threading.Tasks;
using Marten;
using MediatR;
using PaymentService.Domain;
using PolicyService.Api.Events;

namespace PaymentService.Listeners;

public class PolicyTerminatedHandler : INotificationHandler<PolicyTerminated>
{
    private readonly IDocumentStore documentStore;

    public PolicyTerminatedHandler(IDocumentStore documentStore)
    {
        this.documentStore = documentStore;
    }

    public async Task Handle(PolicyTerminated notification, CancellationToken cancellationToken)
    {
        // Open a dedicated session per event to avoid disposing a shared scoped session
        // (fix ObjectDisposedException: 'This session has been disposed')
        await using (var session = documentStore.LightweightSession())
        {
            var policyAccount = await session
                .Query<PolicyAccount>()
                .FirstOrDefaultAsync(p => p.PolicyNumber == notification.PolicyNumber, cancellationToken);

            if (policyAccount == null) return;

            policyAccount.Close(notification.PolicyTo, notification.AmountToReturn);

            session.Update(policyAccount);

            await session.SaveChangesAsync(cancellationToken);
        }
    }
}
