namespace InsureX.Application.DTOs;

// Stub DTOs so code compiles
public class CreateComplianceRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class RuleEvaluationResult
{
    public bool IsPassed { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class PolicySummaryDto
{
    public int ActivePolicies { get; set; }
    public int ExpiredPolicies { get; set; }
}

public class ActivityDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
}

// Prevent duplicate PagedResult
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
}