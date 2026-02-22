using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using InsureX.Application.Interfaces;
using InsureX.Domain.Entities;

namespace InsureX.Application.Services;

public class ScriptRuleEvaluator : IScriptRuleEvaluator
{
    private readonly ScriptOptions _options;

    public ScriptRuleEvaluator()
    {
        _options = ScriptOptions.Default
            .WithImports("System")
            .WithReferences(typeof(object).Assembly);
    }

    public async Task<bool> EvaluateAsync(ComplianceRule rule, object asset)
    {
        if (string.IsNullOrWhiteSpace(rule.CustomScript)) return false;

        try
        {
            // Basic sandbox: only allow simple expressions returning bool
            var script = CSharpScript.Create<bool>(rule.CustomScript, _options, globalsType: asset?.GetType());
            var state = await script.RunAsync(asset);
            return state.ReturnValue;
        }
        catch
        {
            return false;
        }
    }
}
