using System.Diagnostics.CodeAnalysis;

namespace Helpers;

public static class IEnumerableExtensions
{
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? self)
        => self == null || self.IsEmpty();

    public static bool IsNotNullOrEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? self)
        => !self.IsNullOrEmpty();

    public static bool IsEmpty<T>(this IEnumerable<T> self)
        => !self.Any();

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? self)
        => self ?? Enumerable.Empty<T>();

    public static IEnumerable<(int Index, TResult Item)> Enumerated<TSource, TResult>(this IEnumerable<TSource> self, Func<TSource, TResult> selector)
        => self.Select((t, index) => (index, selector(t)));

    public static IEnumerable<(int Index, TSource Item)> Enumerated<TSource>(this IEnumerable<TSource> self)
        => self.Enumerated(e => e);

    public static IEnumerable<T> Values<T>(this IEnumerable<T?> self) where T : struct
        => self.Where(t => t.HasValue)
               .Select(t => t!.Value);

    public static IEnumerable<TResult> Bind<TSource, TResult>(
        this IEnumerable<TSource> self,
        Func<IEnumerable<TSource>, IEnumerable<TResult>> f)
    {
        return !self.Any() ? Enumerable.Empty<TResult>() : f(self);
    }

    public static async Task<IEnumerable<TResult>> BindAsync<TSource, TResult>(
        this IEnumerable<TSource> self,
        Func<IEnumerable<TSource>, Task<IEnumerable<TResult>>> f)
    {
        return !self.Any() ? Enumerable.Empty<TResult>() : await f(self);
    }
}