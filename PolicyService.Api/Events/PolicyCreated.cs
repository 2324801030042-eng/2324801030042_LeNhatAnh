using MediatR;
using PolicyService.Api.Commands.Dtos;

namespace PolicyService.Api.Events;

public class PolicyCreated : INotification
{
    public virtual string PolicyNumber { get; set; } = string.Empty;

    public virtual string ProductCode { get; set; } = string.Empty;

    public virtual DateTime PolicyFrom { get; set; }

    public virtual DateTime PolicyTo { get; set; }

    public virtual PersonDto PolicyHolder { get; set; } = new();

    public virtual decimal TotalPremium { get; set; }

    public virtual string AgentLogin { get; set; } = string.Empty;
}
