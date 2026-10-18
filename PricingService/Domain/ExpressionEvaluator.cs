using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NCalc;

namespace PricingService.Domain;

/// <summary>
/// Expression evaluator using NCalc library.
/// Evaluates expressions like "(NUM_OF_ADULTS) * (DESTINATION == \"EUR\" ? 26.00M : 34.00M)"
/// against a dictionary of variables.
/// </summary>
public static class ExpressionEvaluator
{
    /// <summary>
    /// Evaluates a boolean condition expression.
    /// </summary>
    public static bool EvaluateCondition(string expression, Dictionary<string, object> variables)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return true;

        try
        {
            var processedExpr = PreprocessExpression(expression);
            var expr = new Expression(processedExpr);
            expr.Parameters.Clear();
            foreach (var kvp in variables)
            {
                expr.Parameters[kvp.Key] = kvp.Value;
            }
            expr.EvaluateParameter += (name, args) =>
            {
                if (variables.TryGetValue(name, out var value))
                {
                    args.Result = value;
                }
            };
            var result = expr.Evaluate();
            return Convert.ToBoolean(result);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Evaluates a numeric expression.
    /// </summary>
    public static decimal EvaluateExpression(string expression, Dictionary<string, object> variables)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return 0;

        try
        {
            var processedExpr = PreprocessExpression(expression);
            var expr = new Expression(processedExpr);
            expr.Parameters.Clear();
            foreach (var kvp in variables)
            {
                expr.Parameters[kvp.Key] = kvp.Value;
            }
            expr.EvaluateParameter += (name, args) =>
            {
                if (variables.TryGetValue(name, out var value))
                {
                    args.Result = value;
                }
            };
            var result = expr.Evaluate();
            return Convert.ToDecimal(result);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Preprocesses expression to be compatible with NCalc:
    /// - Removes 'M' suffix from decimal literals
    /// </summary>
    private static string PreprocessExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return expression;

        // Remove 'M' suffix from decimal literals (e.g., 26.00M -> 26.00)
        // NCalc 6.x supports ternary operator natively, so no need to convert
        var processed = DecimalSuffixRegex.Replace(expression, "$1");
        return processed;
    }

    private static readonly Regex DecimalSuffixRegex = new(@"(\d+\.?\d*)M\b", RegexOptions.Compiled);
}