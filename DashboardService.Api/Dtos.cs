namespace DashboardService.Api.Queries.Dtos;

// Stub Phần 2: DTO cho Dashboard (tổng hợp từ nhiều service qua API Gateway).
public class TotalSalesDto
{
    public decimal PremiumAmount { get; set; }
    public long PoliciesCount { get; set; }
}

public class PeriodSalesDto
{
    public string Period { get; set; } = string.Empty;
    public TotalSalesDto Sales { get; set; } = new();
}

public enum TimeUnit
{
    Day,
    Month,
    Year
}
