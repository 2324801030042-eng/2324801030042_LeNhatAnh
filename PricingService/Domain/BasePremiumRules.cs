using System;
using System.Collections.Generic;
using System.Linq;

namespace PricingService.Domain;

public class BasePremiumRules
{
    private readonly List<BasePriceRule> _rules = new();

    public void AddBasePriceRule(string coverCode, string condition, string expression)
    {
        _rules.Add(new BasePriceRule(coverCode, condition, expression));
    }

    public Dictionary<string, decimal> CalculateBasePrices(Calculation calculation)
    {
        var results = new Dictionary<string, decimal>();
        var subject = calculation.Subject;

        foreach (var coverCode in calculation.SelectedCovers)
        {
            var applicableRules = _rules.Where(r => r.CoverCode == coverCode).ToList();
            decimal coverPrice = 0;

            foreach (var rule in applicableRules)
            {
                if (string.IsNullOrEmpty(rule.Condition) || ExpressionEvaluator.EvaluateCondition(rule.Condition, subject))
                {
                    var price = ExpressionEvaluator.EvaluateExpression(rule.Expression, subject);
                    coverPrice += price;
                }
            }

            if (coverPrice > 0)
            {
                results[coverCode] = coverPrice;
            }
        }

        return results;
    }

    private class BasePriceRule
    {
        public string CoverCode { get; }
        public string Condition { get; }
        public string Expression { get; }

        public BasePriceRule(string coverCode, string condition, string expression)
        {
            CoverCode = coverCode;
            Condition = condition;
            Expression = expression;
        }
    }
}