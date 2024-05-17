using FluentValidation;

namespace Helpers.FluentValidation;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, int> NonNegative<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
                    .Must(n => n >= 0)
                    .WithMessage("{PropertyName} must be a positive number or zero.");
    }

    public static IRuleBuilderOptions<T, int> Positive<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
                    .Must(n => n > 0)
                    .WithMessage("{PropertyName} must be a positive number.");
    }
}
