using MediatR;

namespace Dorise.Incentive.Domain.Common;

/// <summary>
/// Base class for all domain events.
/// Domain events represent something significant that happened in the domain.
/// "Me fail English? That's unpossible!" - But domain events never fail!
/// </summary>
public abstract class DomainEvent : INotification
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
