using System;
using System.Linq.Expressions;
using LinqToDB;

namespace Backend.DataLayer
{
    public static class DataExtensions
    {
        public static int InsertId<T>(this IDataContext context, T obj)
        {
            return Convert.ToInt32(context.InsertWithIdentity(obj));
        }

        public const int SchoolStartMonth = 7;
        public const int SchoolEndMonth = 6;

        [ExpressionMethod(nameof(InSchoolYearImp))]
        public static bool InSchoolYear(this DateTime date, int year)
        {
            return date >= new DateTime(year, SchoolStartMonth, 1) && date <= new DateTime(year - 1, SchoolEndMonth, 1);
        }

        public static Expression<Func<DateTime, int, bool>> InSchoolYearImp()
        {
            return (date, year) => date.Between(Sql.ToSql(new DateTime(year, SchoolStartMonth, 1)),
                Sql.ToSql(new DateTime(year + 1, SchoolEndMonth, 1)));
        }

        [ExpressionMethod(nameof(InSchoolYearNullImp))]
        public static bool InSchoolYear(this DateTime? date, int year)
        {
            return date >= new DateTime(year, SchoolStartMonth, 1) && date <= new DateTime(year - 1, SchoolEndMonth, 1);
        }

        public static Expression<Func<DateTime?, int, bool>> InSchoolYearNullImp()
        {
            return (date, year) => date.Between(Sql.ToSql(new DateTime(year, SchoolStartMonth, 1)),
                Sql.ToSql(new DateTime(year + 1, SchoolEndMonth, 1)));
        }

        public static int SchoolYear(this DateTime date)
        {
            return date.Month >= 7 ? date.Year : date.Year - 1;
        }

        /// <summary>
        /// this checks to see if the given date is in the school year and school month
        /// after it's been divided up into blocks defined by the month range
        /// the school year gets divided up into blocks, so a 12 month block size would be 1 block per year
        /// 6 month would be 2 blocks etc
        /// the date will match if it's in the same block as the year and month provided
        /// 
        /// it starts counting from the end of the year and goes back from there
        /// 
        /// the month is the school month matching the school year
        /// so school year 2017 with school month 1 is actually jan 2018
        /// </summary>
        /// <param name="date"></param>
        /// <param name="schoolYear"></param>
        /// <param name="schoolMonth"></param>
        /// <param name="monthBlockSize"></param>
        /// <returns></returns>
        [ExpressionMethod(nameof(InMonthBlockImp))]
        public static bool InMonthBlock(this DateTime? date, int schoolYear, int schoolMonth, int monthBlockSize)
        {
            return date.Between(new DateTime(schoolYear + 1, SchoolEndMonth + 1, 1).AddMonths(
                    -(MonthsFromEnd(schoolMonth) / monthBlockSize * monthBlockSize + monthBlockSize)),
                new DateTime(schoolYear + 1, SchoolEndMonth, 30).AddMonths(
                    -(MonthsFromEnd(schoolMonth) / monthBlockSize * monthBlockSize)));
        }

        public static Expression<Func<DateTime?, int, int, int, bool>> InMonthBlockImp()
        {
            return (date, schoolYear, schoolMonth, monthBlockSize) => date.Between(
                new DateTime(schoolYear + 1, SchoolEndMonth + 1, 1).AddMonths(
                    -(MonthsFromEnd(schoolMonth) / monthBlockSize * monthBlockSize + monthBlockSize)),
                new DateTime(schoolYear + 1, SchoolEndMonth, 30).AddMonths(
                    -(MonthsFromEnd(schoolMonth) / monthBlockSize * monthBlockSize)));
        }

        [ExpressionMethod(nameof(MonthsFromEndImp))]
        public static int MonthsFromEnd(int month)
        {
            return month > SchoolEndMonth ? SchoolEndMonth + (12 - month) : Math.Abs(month - SchoolEndMonth);
        }

        public static Expression<Func<int, int>> MonthsFromEndImp()
        {
            return month => month > SchoolEndMonth ? SchoolEndMonth + (12 - month) : Math.Abs(month - SchoolEndMonth);
        }

        public static DateTime SchoolYearStartDate(int year)
        {
            return new DateTime(year, SchoolStartMonth, 1);
        }

        [Sql.Expression("DATE_PART('day', {1} - {0})", PreferServerSide = true)]
        public static int? DayDiff(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null || endDate == null)
                return null;
            return (int) (endDate - startDate).Value.TotalDays;
        }

        [Sql.Expression("DateTime({0}, {1} || ' Month')", PreferServerSide = true)]
        public static DateTime AddMonths(DateTime date, int months)
        {
            return date.AddMonths(months);
        }
    }
}