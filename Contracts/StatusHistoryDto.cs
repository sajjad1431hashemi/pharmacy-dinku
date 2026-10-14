namespace PharmacyMiddleware.Contracts;

public sealed class StatusHistoryDto
{
    public string? Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserName { get; set; }
    public string? Note { get; set; }
}