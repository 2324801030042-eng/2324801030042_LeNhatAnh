namespace PolicyService.Api.Commands.Dtos;

// Stub tối thiểu Phần 2: định nghĩa API (ports) theo quy tắc Task #1.
// Chi tiết CQRS/MediatR handler nằm ở project Implementation (Phần 3).
public enum QuestionType
{
    Numeric,
    Choice
}

public abstract class QuestionAnswer
{
    public string QuestionCode { get; set; } = string.Empty;
    public abstract QuestionType QuestionType { get; }
}

public class ChoiceQuestionAnswer : QuestionAnswer
{
    public override QuestionType QuestionType => QuestionType.Choice;
    public string? Answer { get; set; }
}

public class NumericQuestionAnswer : QuestionAnswer
{
    public override QuestionType QuestionType => QuestionType.Numeric;
    public decimal Answer { get; set; }
}

public class PersonDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
}

public class AddressDto
{
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
}
