using System;
using Backend.Entities.Helper;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class DateRangeTests
    {
        private DateTime Date(int year)
        {
            return new DateTime(2000 + year, 1, 1);
        }

        [Fact]
        public void IncludesRageWorks()
        {
            DateRange range1 = (Date(1), Date(5));
            DateRange range2 = (Date(3), Date(4));
            DateRange range3 = (Date(7), Date(8));
            range1.Includes(range2).ShouldBeTrue();
            range1.Includes(range3).ShouldBeFalse();
        }

        [Fact]
        public void OverlapWorks()
        {
            DateRange range1 = (Date(1), Date(3));
            DateRange range2 = (Date(2), Date(5));
            DateRange range3 = (Date(7), Date(8));

            range1.Overlaps(range2).ShouldBeTrue();
            range2.Overlaps(range1).ShouldBeTrue();
            range1.Overlaps(range1).ShouldBeTrue();
            range2.Overlaps(range2).ShouldBeTrue();

            range1.Overlaps(range3).ShouldBeFalse();
            range2.Overlaps(range3).ShouldBeFalse();
            range3.Overlaps(range1).ShouldBeFalse();
            range3.Overlaps(range2).ShouldBeFalse();
        }

        [Fact]
        public void CombineOverlappingWorks()
        {
            var expectedStart = Date(1);
            var expectedEnd = Date(5);
            DateRange range1 = (expectedStart, Date(3));
            DateRange range2 = (Date(2), expectedEnd);
            var (actualResult, nullResult) = range1.Combine(range2);
            actualResult.ShouldBe(new DateRange(expectedStart, expectedEnd));
            nullResult.ShouldBeNull();
        }

        [Fact]
        public void CombineNonOverlappingWorks()
        {
            DateRange range1 = (Date(1), Date(5));
            DateRange range3 = (Date(7), Date(8));

            range1.Combine(range3).ShouldBe((range1, range3));
            range3.Combine(range1).ShouldBe((range3, range1));
        }

        [Fact]
        public void CombineSameWorks()
        {
            DateRange range1 = (Date(1), Date(5));
            DateRange range2 = (Date(1), Date(5));
            var (actualResult, nullResult) = range1.Combine(range2);
            actualResult.ShouldBe(range1);
            nullResult.ShouldBeNull();
        }

        [Fact]
        public void CombineMultipleWorks()
        {
            var start = Date(1);
            var end = Date(5);
            DateRange range1 = (start, Date(3));
            DateRange range2 = (Date(2), end);
            DateRange range3 = (Date(7), Date(8));

            DateRange.Combine(new[] {range1, range2, range3})
                .ShouldBe(new[] {new DateRange(start, end), range3});
        }
    }
}