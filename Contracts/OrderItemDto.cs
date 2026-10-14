namespace PharmacyMiddleware.Contracts;

public sealed class OrderItemDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int Quantity { get; set; }
    public string? Dosage { get; set; }
    public double? UnitPrice { get; set; }
}