namespace zenvy.application.DTOs.Payments;

public class PaymentRequest
{
    public long OrderId { get; set; }
    public int PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionRef { get; set; }
    public string? Status { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
}

public class PaymentResponse
{
    public long PaymentId { get; set; }
    public long OrderId { get; set; }
    public int PaymentMethodId { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionRef { get; set; }
    public string? Status { get; set; }
    public DateTime PaymentDate { get; set; }
}
