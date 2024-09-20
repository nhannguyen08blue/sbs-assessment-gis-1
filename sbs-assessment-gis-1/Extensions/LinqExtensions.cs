namespace sbs_assessment_gis_1.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource,bool> predicate)
    {
        if (condition)
            return source.Where(predicate);

        return source;
    }
}
