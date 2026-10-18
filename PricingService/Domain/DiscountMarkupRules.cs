using System;
using System.Collections.Generic;
using System.Linq;

namespace PricingService.Domain;

public class DiscountMarkupRules
{
    private readonly List<MarkupRule> _rules = new();

    public void AddPercentMarkup(string condition, decimal percent)
    {
        _rules.Add(new MarkupRule(condition, percent));
    }

    public decimal ApplyMarkups(decimal basePrice, Dictionary<string, object> subject)
    {
        decimal result = basePrice;
        foreach (var rule in _rules)
        {
            if (ExpressionEvaluator.EvaluateCondition(rule.Condition, subject))
            {
                result *= rule.Percent;
            }
        }
        return result;
    }

    private class MarkupRule
    {
        public string Condition { get; }
        public decimal Percent { get; }

        public MarkupRule(string condition, decimal percent)
        {
            Condition = condition;
            Percent = percent;
        }
    }
}