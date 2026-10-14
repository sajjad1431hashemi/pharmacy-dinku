namespace PharmacyMiddleware.Contracts;

public sealed class OrderAttachmentDto
{
    public Guid Id { get; set; }
    public string? FileType { get; set; }
    public string? Url { get; set; }
    public string? FileName { get; set; }
    public long FileSizeBytes { get; set; }
}