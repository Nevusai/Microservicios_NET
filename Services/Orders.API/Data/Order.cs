namespace Orders.API.Data;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = "Created";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
