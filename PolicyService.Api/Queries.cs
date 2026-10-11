namespace PolicyService.Api.Queries;

// Stub Phần 2: truy vấn mà PolicyService có thể trả lời.
public class PolicyDetailsDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string PolicyHolder { get; set; } = string.Empty;
    public decimal TotalPremium { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public List<string> Covers { get; set; } = new();
}

public class GetPolicyDetailsQueryResult
{
    public PolicyDetailsDto Policy { get; set; } = new();
}
