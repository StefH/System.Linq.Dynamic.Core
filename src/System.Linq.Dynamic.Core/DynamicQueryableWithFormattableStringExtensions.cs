using JetBrains.Annotations;
using System.Collections;
using System.Text.RegularExpressions;

namespace System.Linq.Dynamic.Core
{
#if NET46_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER  || NETSTANDARD1_3_OR_GREATER || UAP10_0
    public static class DynamicQueryableWithFormattableStringExtensions
    {
        public static IQueryable WhereInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Where(source, config, predicateStr, args);
        }

        public static IQueryable WhereInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Where(source, predicateStr, args);
        }

        public static IQueryable<TSource> WhereInterpolated<TSource>([NotNull] this IQueryable<TSource> source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Where(source, config, predicateStr, args);
        }

        public static IQueryable<TSource> WhereInterpolated<TSource>([NotNull] this IQueryable<TSource> source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Where(source, predicateStr, args);
        }

        [PublicAPI]
        public static bool AllInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.All(source, predicateStr, args);
        }

        [PublicAPI]
        public static bool AllInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.All(source, config, predicateStr, args);
        }

        [PublicAPI]
        public static bool AnyInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Any(source, config, predicateStr, args);
        }

        public static bool AnyInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Any(source, predicateStr, args);
        }

        [PublicAPI]
        public static double AverageInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Average(source, config, predicateStr, args);
        }

        [PublicAPI]
        public static double AverageInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Average(source, predicateStr, args);
        }

        public static dynamic SingleInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Single(source, config, predicateStr, args);
        }

        public static dynamic SingleInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Single(source, predicateStr, args);
        }

        public static dynamic SingleOrDefaultInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.SingleOrDefault(source, config, predicateStr, args);
        }

        public static dynamic SingleOrDefaultInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.SingleOrDefault(source, predicateStr, args);
        }

        public static IQueryable SkipWhileInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.SkipWhile(source, config, predicateStr, args);
        }

        public static IQueryable SkipWhileInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.SkipWhile(source, predicateStr, args);
        }

        public static IQueryable TakeWhileInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.TakeWhile(source, config, predicateStr, args);
        }

        public static IQueryable TakeWhileInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.TakeWhile(source, predicateStr, args);
        }

        [PublicAPI]
        public static object SumInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Sum(source, config, predicateStr, args);
        }

        [PublicAPI]
        public static object SumInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Sum(source, predicateStr, args);
        }

        [PublicAPI]
        public static int CountInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Count(source, config, predicateStr, args);
        }

        public static int CountInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Count(source, predicateStr, args);
        }

        public static dynamic FirstInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.First(source, config, predicateStr, args);
        }

        public static dynamic FirstInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.First(source, predicateStr, args);
        }

        public static dynamic FirstOrDefaultInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.FirstOrDefault(source, config, predicateStr, args);
        }

        public static dynamic FirstOrDefaultInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.FirstOrDefault(source, predicateStr, args);
        }

        public static dynamic LastInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Last(source, config, predicateStr, args);
        }

        public static dynamic LastInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Last(source, predicateStr, args);
        }

        public static dynamic LastOrDefaultInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.LastOrDefault(source, config, predicateStr, args);
        }

        public static dynamic LastOrDefaultInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.LastOrDefault(source, predicateStr, args);
        }

        [PublicAPI]
        public static long LongCountInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.LongCount(source, config, predicateStr, args);
        }

        public static long LongCountInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.LongCount(source, predicateStr, args);
        }

        [PublicAPI]
        public static object MaxInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Max(source, config, predicateStr, args);
        }

        [PublicAPI]
        public static object MaxInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Max(source, predicateStr, args);
        }

        [PublicAPI]
        public static object MinInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Min(source, config, predicateStr, args);
        }

        [PublicAPI]
        public static object MinInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return DynamicQueryableExtensions.Min(source, predicateStr, args);
        }

        public static IQueryable SelectInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.Select(source, config, selectorStr, args);
        }

        public static IQueryable SelectInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] Type resultType, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.Select(source, config, resultType, selectorStr, args);
        }

        public static IQueryable SelectInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.Select(source, selectorStr, args);
        }

        public static IQueryable<TResult> SelectInterpolated<TResult>([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.Select<TResult>(source, config, selectorStr, args);
        }

        public static IQueryable<TResult> SelectInterpolated<TResult>([NotNull] this IQueryable source, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.Select<TResult>(source, selectorStr, args);
        }

        public static IQueryable SelectManyInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString collectionSelector, [NotNull] FormattableString resultSelector)
        {
            string collectionSelectorStr = ParseFormattableString(collectionSelector, out object[] collectionSelectorArgs);
            string resultSelectorStr = ParseFormattableString(resultSelector, out object[] resultSelectorArgs);
            return DynamicQueryableExtensions.SelectMany(source, config, collectionSelectorStr, resultSelectorStr, collectionSelectorArgs, resultSelectorArgs);
        }

        public static IQueryable SelectManyInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.SelectMany(source, config, selectorStr, args);
        }

        public static IQueryable SelectManyInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] Type resultType, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.SelectMany(source, config, resultType, selectorStr, args);
        }

        public static IQueryable SelectManyInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString collectionSelector, [NotNull] FormattableString resultSelector)
        {
            string collectionSelectorStr = ParseFormattableString(collectionSelector, out object[] collectionSelectorArgs);
            string resultSelectorStr = ParseFormattableString(resultSelector, out object[] resultSelectorArgs);
            return DynamicQueryableExtensions.SelectMany(source, collectionSelectorStr, collectionSelectorArgs, resultSelectorStr, resultSelectorArgs);
        }

        public static IQueryable SelectManyInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.SelectMany(source, selectorStr, args);
        }

        public static IQueryable SelectManyInterpolated([NotNull] this IQueryable source, [NotNull] Type resultType, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.SelectMany(source, resultType,selectorStr, args);
        }

        public static IQueryable<TResult> SelectManyInterpolated<TResult>([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.SelectMany<TResult>(source, config, selectorStr, args);
        }

        public static IQueryable<TResult> SelectManyInterpolated<TResult>([NotNull] this IQueryable source, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return DynamicQueryableExtensions.SelectMany<TResult>(source, selectorStr, args);
        }

        public static IOrderedQueryable OrderByInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString ordering, IComparer comparer)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.OrderBy(source, config, orderingStr, comparer, args);
        }

        public static IOrderedQueryable OrderByInterpolated([NotNull] this IQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString ordering)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.OrderBy(source, config, orderingStr, args);
        }

        public static IOrderedQueryable OrderByInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString ordering, IComparer comparer)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.OrderBy(source, orderingStr, comparer, args);
        }

        public static IOrderedQueryable OrderByInterpolated([NotNull] this IQueryable source, [NotNull] FormattableString ordering)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.OrderBy(source, orderingStr, args);
        }

        public static IOrderedQueryable<TSource> OrderByInterpolated<TSource>([NotNull] this IQueryable<TSource> source, [NotNull] ParsingConfig config, [NotNull] FormattableString ordering, IComparer comparer)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.OrderBy(source, config, orderingStr, comparer,args);
        }

        public static IOrderedQueryable<TSource> OrderByInterpolated<TSource>([NotNull] this IQueryable<TSource> source, [NotNull] ParsingConfig config, [NotNull] FormattableString ordering)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.OrderBy(source, config, orderingStr, args);
        }

        public static IOrderedQueryable<TSource> OrderByInterpolated<TSource>([NotNull] this IQueryable<TSource> source, [NotNull] FormattableString ordering, IComparer comparer)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.OrderBy(source, orderingStr, comparer, args);
        }

        public static IOrderedQueryable<TSource> OrderByInterpolated<TSource>([NotNull] this IQueryable<TSource> source, [NotNull] FormattableString ordering)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.OrderBy(source, orderingStr, args);
        }

        public static IOrderedQueryable ThenByInterpolated([NotNull] this IOrderedQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString ordering, IComparer comparer)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.ThenBy(source, config, orderingStr, comparer, args);
        }

        public static IOrderedQueryable ThenByInterpolated([NotNull] this IOrderedQueryable source, [NotNull] ParsingConfig config, [NotNull] FormattableString ordering)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.ThenBy(source, config, orderingStr, args);
        }

        public static IOrderedQueryable ThenByInterpolated([NotNull] this IOrderedQueryable source, [NotNull] FormattableString ordering, IComparer comparer)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.ThenBy(source, orderingStr, comparer, args);
        }

        public static IOrderedQueryable ThenByInterpolated([NotNull] this IOrderedQueryable source, [NotNull] FormattableString ordering)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.ThenBy(source, orderingStr, args);
        }

        public static IOrderedQueryable<TSource> ThenByInterpolated<TSource>([NotNull] this IOrderedQueryable<TSource> source, [NotNull] ParsingConfig config, [NotNull] FormattableString ordering, IComparer comparer)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.ThenBy(source, config, orderingStr, comparer, args);
        }

        public static IOrderedQueryable<TSource> ThenByInterpolated<TSource>([NotNull] this IOrderedQueryable<TSource> source, [NotNull] ParsingConfig config, [NotNull] FormattableString ordering)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.ThenBy(source, config, orderingStr, args);
        }

        public static IOrderedQueryable<TSource> ThenByInterpolated<TSource>([NotNull] this IOrderedQueryable<TSource> source, [NotNull] FormattableString ordering, IComparer comparer)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.ThenBy(source,  orderingStr, comparer, args);
        }

        public static IOrderedQueryable<TSource> ThenByInterpolated<TSource>([NotNull] this IOrderedQueryable<TSource> source, [NotNull] FormattableString ordering)
        {
            string orderingStr = ParseFormattableString(ordering, out object[] args);
            return DynamicQueryableExtensions.ThenBy(source, orderingStr, args);
        }

        private static string ParseFormattableString(FormattableString predicate,out object[] args)
        {
            string predicateStr = predicate.Format;
            predicateStr = Regex.Replace(predicateStr, @"{(\d+)}", "@$1");//replace {0} with @0
            args = predicate.GetArguments();
            return predicateStr;            
        }
    }
#endif

}
