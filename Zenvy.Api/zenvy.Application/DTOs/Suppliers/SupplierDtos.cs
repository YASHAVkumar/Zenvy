namespace zenvy.application.DTOs.Suppliers;

public class SupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool Status { get; set; } = true;
}

public class SupplierResponse
{
    public int SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
