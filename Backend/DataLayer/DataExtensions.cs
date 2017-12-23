using System;
using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Identity;
using LinqToDB.SqlProvider;
using LinqToDB.SqlQuery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backend.DataLayer
{
    public static class DataExtensions
    {
        public static int InsertId<T>(this IDataContext context, T obj)
        {
            return Convert.ToInt32(context.InsertWithIdentity(obj));
        }

        [ExpressionMethod(nameof(InSchoolYearImp))]
        public static bool InSchoolYear(this DateTime date, int year)
        {
            return date >= new DateTime(year, 7, 1) && date <= new DateTime(year - 1, 6, 1);
        }

        public static Expression<Func<DateTime, int, bool>> InSchoolYearImp()
        {
            return (date, year) => date.Between(Sql.ToSql(new DateTime(year, 7, 1)), Sql.ToSql(new DateTime(year + 1, 6, 1)));
        }
        
        [ExpressionMethod(nameof(InSchoolYearNullImp))]
        public static bool InSchoolYear(this DateTime? date, int year)
        {
            return date >= new DateTime(year, 7, 1) && date <= new DateTime(year - 1, 6, 1);
        }

        public static Expression<Func<DateTime?, int, bool>> InSchoolYearNullImp()
        {
            return (date, year) => date.Between(Sql.ToSql(new DateTime(year, 7, 1)), Sql.ToSql(new DateTime(year + 1, 6, 1)));
        }

        public static int SchoolYear(this DateTime date)
        {
            return date.Month >= 7 ? date.Year : date.Year - 1;
        }

        [Sql.Expression("DATE_PART('day', {1} - {0})", PreferServerSide = true)]
        public static int? DayDiff(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
                return null;
            return (int) (endDate - startDate).Value.TotalDays;
        }
    }
}