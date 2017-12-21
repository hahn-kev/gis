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
            return (date, year) => date >= new DateTime(year, 7, 1) && date <= new DateTime(year + 1, 6, 1);
        }

        public static int SchoolYear(this DateTime date)
        {
            if (date.Month >= 7) return date.Year;
            return date.Year - 1;
        }

        [Sql.Expression("DATE_PART('{0}', {2} - {1})", PreferServerSide = true)]
        public static int? DateDiff(Sql.DateParts part, DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
                return null;

            switch (part)
            {
                case Sql.DateParts.Day         : return (int)(endDate - startDate).Value.TotalDays;
                case Sql.DateParts.Hour        : return (int)(endDate - startDate).Value.TotalHours;
                case Sql.DateParts.Minute      : return (int)(endDate - startDate).Value.TotalMinutes;
                case Sql.DateParts.Second      : return (int)(endDate - startDate).Value.TotalSeconds;
                case Sql.DateParts.Millisecond : return (int)(endDate - startDate).Value.TotalMilliseconds;
            }

            throw new InvalidOperationException();
        }
    }
}