using PolicySearchService.Api.Queries.Dtos;

namespace PolicySearchService.Api.Queries;

// Stub Phần 2: truy vấn tìm kiếm chính sách.
public class FindPolicyResult
{
    public List<PolicyDto> Policies { get; set; } = new();
}
