namespace Helpers;

public static class StringExtensions
{
    public static string ValueOr(this string? self, string alternative)
    {
        return string.IsNullOrEmpty(self) ? alternative : self;
    }
}