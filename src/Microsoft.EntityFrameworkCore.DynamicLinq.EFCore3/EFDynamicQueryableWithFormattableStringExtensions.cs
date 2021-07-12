#if EFCORE
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
#else
using System.Data.Entity.Infrastructure;
#endif
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Validation;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using System.Text.RegularExpressions;

#if EFCORE
namespace Microsoft.EntityFrameworkCore.DynamicLinq
#else
namespace EntityFramework.DynamicLinq
#endif
{
#if EFCORE || (NET46_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NETSTANDARD1_3_OR_GREATER || UAP10_0)
    public static class EFDynamicQueryableWithFormattableStringExtensions
    {
        
        private static string ParseFormattableString(FormattableString predicate, out object[] args)
        {
            string predicateStr = predicate.Format;
            predicateStr = Regex.Replace(predicateStr, @"{(\d+)}", "@$1");//replace {0} with @0
            args = predicate.GetArguments();
            return predicateStr;
        }

        [PublicAPI]
        public static Task<bool> AllInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.AllAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<bool> AllInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.AllAsync(source, predicateStr, cancellationToken, args);
        }

        [PublicAPI]
        public static Task<bool> AnyInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.AnyAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<bool> AnyInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate, CancellationToken cancellationToken = default(CancellationToken))
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.AnyAsync(source, predicateStr, cancellationToken, args);
        }

        [PublicAPI]
        public static Task<double> AverageInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.AverageAsync(source, selectorStr, args);
        }


        [PublicAPI]
        public static Task<double> AverageInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.AverageAsync(source, selectorStr, cancellationToken, args);
        }

        [PublicAPI]
        public static Task<int> CountInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.CountAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<int> CountInterpolatedAsync([NotNull] this IQueryable source, CancellationToken cancellationToken, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.CountAsync(source, cancellationToken,predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> FirstInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.FirstAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> FirstInterpolatedAsync([NotNull] this IQueryable source, CancellationToken cancellationToken, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.FirstAsync(source, cancellationToken, predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> FirstOrDefaultInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.FirstOrDefaultAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> FirstOrDefaultInterpolatedAsync([NotNull] this IQueryable source, CancellationToken cancellationToken, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.FirstOrDefaultAsync(source, cancellationToken,predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> LastInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.LastAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> LastInterpolatedAsync([NotNull] this IQueryable source, CancellationToken cancellationToken, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.LastAsync(source, cancellationToken,predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> LastOrDefaultInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.LastOrDefaultAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> LastOrDefaultInterpolatedAsync([NotNull] this IQueryable source, CancellationToken cancellationToken, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.LastOrDefaultAsync(source, cancellationToken, predicateStr, args);
        }

        [PublicAPI]
        public static Task<long> LongCountInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.LongCountAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<long> LongCountInterpolatedAsync([NotNull] this IQueryable source, CancellationToken cancellationToken, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.LongCountAsync(source, cancellationToken,predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> SingleOrDefaultInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.SingleOrDefaultAsync(source, predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> SingleOrDefaultInterpolatedAsync([NotNull] this IQueryable source, CancellationToken cancellationToken, [NotNull] FormattableString predicate)
        {
            string predicateStr = ParseFormattableString(predicate, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.SingleOrDefaultAsync(source, cancellationToken,predicateStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> SumInterpolatedAsync([NotNull] this IQueryable source, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.SumAsync(source, selectorStr, args);
        }

        [PublicAPI]
        public static Task<dynamic> SumInterpolatedAsync([NotNull] this IQueryable source, CancellationToken cancellationToken, [NotNull] FormattableString selector)
        {
            string selectorStr = ParseFormattableString(selector, out object[] args);
            return EntityFrameworkDynamicQueryableExtensions.SumAsync(source, cancellationToken,selectorStr, args);
        }
        
    }
#endif
}
