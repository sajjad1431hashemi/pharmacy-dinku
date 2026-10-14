namespace PharmacyMiddleware.Contracts;

public sealed class OrderDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TrackingCode { get; set; }
    public string? RequestType { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? PatientName { get; set; }
    public string? PatientMobile { get; set; }
    public string? PatientNationalId { get; set; }

    public string? InsuranceType { get; set; }
    public string? InsuranceTrackingCode { get; set; }

    public string? DeliveryMethod { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? DeliveryNotes { get; set; }
    public string? PatientNotes { get; set; }
    public string? PharmacyNotes { get; set; }

    public double? TotalPrice { get; set; }

    public int? FeedbackRating { get; set; }
    public string? FeedbackComment { get; set; }
    public DateTime? FeedbackSubmittedAt { get; set; }

    public List<OrderItemDto>? Items { get; set; }
    public List<OrderAttachmentDto>? Attachments { get; set; }
    public List<StatusHistoryDto>? StatusHistory { get; set; }
}