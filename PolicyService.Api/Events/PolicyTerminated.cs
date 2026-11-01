using MediatR;

namespace PolicyService.Api.Events;

public class PolicyTerminated : INotification
{
    public virtual string PolicyNumber { get; set; } = string.Empty;

    public virtual DateTime PolicyTo { get; set; }

    public virtual decimal AmountToReturn { get; set; }
}
