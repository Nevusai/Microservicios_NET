namespace SharedModels.Contracts;

public record CreateOrderRequest(int UserId, int ProductId, int Quantity);
