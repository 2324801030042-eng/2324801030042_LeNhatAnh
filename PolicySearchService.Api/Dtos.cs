namespace PolicySearchService.Api.Queries.Dtos;

// Stub Phần 2: DTO cho chức năng tìm kiếm (index trong ElasticSearch ở Phần 3).
public class PolicyDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public DateTime PolicyStartDate { get; set; }
    public DateTime PolicyEndDate { get; set; }
    public string PolicyHolder { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
}
