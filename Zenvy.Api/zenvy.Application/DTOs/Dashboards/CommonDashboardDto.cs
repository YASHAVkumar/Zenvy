namespace zenvy.application.DTOs.Dashboards;

/// <summary>
/// Common dashboard DTOs shared across all dashboards
/// </summary>
public class TrendDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class DashboardResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
