using InsureX.Domain.Entities;

namespace InsureX.Application.Interfaces;

public interface IScriptRuleEvaluator
{
    Task<bool> EvaluateAsync(ComplianceRule rule, object asset);
     Task<bool> EvaluateRuleAsync(string script, Dictionary<string, object> parameters);
}
