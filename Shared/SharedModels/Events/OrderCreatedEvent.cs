namespace SharedModels.Events;

public record OrderCreatedEvent(
    int OrderId,
    int UserId,
    int ProductId,
    int Quantity,
    decimal UnitPrice,
    DateTime CreatedAtUtc
);
