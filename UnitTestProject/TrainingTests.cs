using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using LinqToDB;
using Shouldly;
using Xunit;
using DataExtensions = Backend.DataLayer.DataExtensions;

namespace UnitTestProject
{
    public class TrainingTests
    {
        private readonly ServicesFixture _sf;
        private TrainingRepository _repo;
        private const int StartMonth = DataExtensions.SchoolStartMonth;
        private const int EndMonth = DataExtensions.SchoolEndMonth;

        public TrainingTests()
        {
            _sf = new ServicesFixture();
            _repo = _sf.Get<TrainingRepository>();
        }

        [Fact]
        public void GetsResults()
        {
            var tr = _sf.InsertRequirement();
            var expectedTraining = _sf.InsertTraining(tr.Id, new DateTime(2018, 3, 1));
            var training = _repo.StaffTrainingWithRequirements.ToArray();
            training.ShouldNotBeEmpty();
            training.ShouldHaveSingleItem();
            training.ShouldContain(t => t.Id == expectedTraining.Id);
            var actualTraining = training.Single();
            actualTraining.CompletedDate.ShouldBe(expectedTraining.CompletedDate);
        }

        [Fact]
        public void MonthsToEndOfYearWorks()
        {
            DataExtensions.MonthsFromEnd(6).ShouldBe(0);
            DataExtensions.MonthsFromEnd(5).ShouldBe(1);
            DataExtensions.MonthsFromEnd(4).ShouldBe(2);
            DataExtensions.MonthsFromEnd(3).ShouldBe(3);
            DataExtensions.MonthsFromEnd(2).ShouldBe(4);
            DataExtensions.MonthsFromEnd(1).ShouldBe(5);
            DataExtensions.MonthsFromEnd(12).ShouldBe(6);
            DataExtensions.MonthsFromEnd(11).ShouldBe(7);
            DataExtensions.MonthsFromEnd(10).ShouldBe(8);
            DataExtensions.MonthsFromEnd(9).ShouldBe(9);
        }

        public static IEnumerable<object[]> MonthBlockData()
        {
            IEnumerable<(DateTime date, int month, int monthRatio, bool )> Values()
            {
                yield return (new DateTime(2016, StartMonth, 2), 3, 12, false);
                yield return (new DateTime(2017, EndMonth, 30), 3, 12, false);
                yield return (new DateTime(2017, StartMonth, 1), 3, 12, true);

                yield return (new DateTime(2018, 3, 2), 3, 12, true);

                yield return (new DateTime(2018, StartMonth, 1), 3, 12, false);
                yield return (new DateTime(2018, StartMonth, 2), 3, 12, false);


                yield return (new DateTime(2015, StartMonth, 2), StartMonth, 24, false);
                yield return (new DateTime(2016, EndMonth, 30), StartMonth, 24, false);
                yield return (new DateTime(2016, StartMonth, 1), StartMonth, 24, true);
                yield return (new DateTime(2016, StartMonth, 2), StartMonth, 24, true);

                yield return (new DateTime(2017, StartMonth, 2), StartMonth, 24, true);

                yield return (new DateTime(2018, EndMonth, 30), StartMonth, 24, true);
                yield return (new DateTime(2018, StartMonth, 1), StartMonth, 24, false);


                yield return (new DateTime(2018, EndMonth, 30).AddMonths(-2), EndMonth, 2, false);
                yield return (new DateTime(2018, EndMonth, 1).AddMonths(-1), EndMonth, 2, true);
                yield return (new DateTime(2018, EndMonth, 30).AddMonths(+0), EndMonth, 2, true);
                yield return (new DateTime(2018, EndMonth, 1).AddMonths(+1), EndMonth, 2, false);

                yield return (new DateTime(2017, StartMonth, 30).AddMonths(-1), StartMonth + 1, 2, false);
                yield return (new DateTime(2017, StartMonth, 1).AddMonths(+0), StartMonth + 1, 2, true);
                yield return (new DateTime(2017, StartMonth, 30).AddMonths(+1), StartMonth + 1, 2, true);
                yield return (new DateTime(2017, StartMonth, 1).AddMonths(+2), StartMonth + 1, 2, false);

                yield return (new DateTime(2017, StartMonth, 30).AddMonths(+1), StartMonth + 2, 2, false);
                yield return (new DateTime(2017, StartMonth, 1).AddMonths(+2), StartMonth + 2, 2, true);
                yield return (new DateTime(2017, StartMonth, 30).AddMonths(+3), StartMonth + 2, 2, true);
                yield return (new DateTime(2017, StartMonth, 1).AddMonths(+4), StartMonth + 2, 2, false);

                yield return (new DateTime(2017, StartMonth, 30).AddMonths(-1), StartMonth, 6, false);
                yield return (new DateTime(2017, StartMonth, 1).AddMonths(0), StartMonth, 6, true);
                yield return (new DateTime(2017, StartMonth, 30).AddMonths(5), StartMonth, 6, true);
                yield return (new DateTime(2017, StartMonth, 1).AddMonths(6), StartMonth, 6, false);

                yield return (new DateTime(2017, StartMonth, 30).AddMonths(5), StartMonth + 6, 6, false);
                yield return (new DateTime(2017, StartMonth, 2).AddMonths(6), StartMonth + 6, 6, true);
                yield return (new DateTime(2017, StartMonth, 30).AddMonths(11), StartMonth + 6, 6, true);
                yield return (new DateTime(2017, StartMonth, 1).AddMonths(12), StartMonth + 6, 6, false);
            }

            return Values().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(MonthBlockData))]
        public void InMonthBlockTest(DateTime date, int month, int monthRatio, bool expectedResult)
        {
            var actualResult = ((DateTime?) date).InMonthBlock(2017, month > 12 ? month % 12 : month, monthRatio);
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void SqlDatesGetExecutedProperly()
        {
            var tr = _sf.InsertRequirement(months: 12);
            var dateTimes = _sf.DbConnection.TrainingRequirements.Select(requirement => new[]
            {
                Sql.AsSql(new DateTime(2018, 3, 1).AddMonths(Sql.AsSql(requirement.RenewMonthsCount / -2))),
                new DateTime(2018, 3, 1).AddMonths(requirement.RenewMonthsCount / 2)
            }).First();
            dateTimes.Length.ShouldBe(2);
            var subtactTime = dateTimes[0];
            var addTime = dateTimes[1];
            subtactTime.ShouldBe(new DateTime(2018, 3, 1).AddMonths(tr.RenewMonthsCount / -2));
            addTime.ShouldBe(new DateTime(2018, 3, 1).AddMonths(tr.RenewMonthsCount / 2));
        }

        [Fact]
        public void GetByYearReturnsSomeResults()
        {
            var tr = _sf.InsertRequirement();
            var expectedTraining = _sf.InsertTraining(tr.Id, new DateTime(2018, 3, 1));
            var staffTrainings = _repo.GetStaffTrainingByYearMonth(2017, 7).ToArray();
            staffTrainings.ShouldHaveSingleItem();
            staffTrainings.ShouldContain(training => training.Id == expectedTraining.Id);
        }

        [Fact]
        public void EnsureTrainingAtTheStartOfNextYearDontGetIncluded()
        {
            var tr = _sf.InsertRequirement();
            _sf.InsertTraining(tr.Id, new DateTime(2017, EndMonth, 30));
            var expectedTraining = _sf.InsertTraining(tr.Id, new DateTime(2018, 3, 1));
            _sf.InsertTraining(tr.Id, new DateTime(2018, StartMonth, 1));
            //the tr we're looking at is for 12 months, so the range should be 2017-start to 2018-end
            var staffTrainings = _repo.GetStaffTrainingByYearMonth(2017, 3)
                .Select(training => training.CompletedDate.GetValueOrDefault().ToShortDateString()).ToArray();
            staffTrainings.ShouldHaveSingleItem();

            staffTrainings.ShouldContain(expectedTraining.CompletedDate.GetValueOrDefault().ToShortDateString());
        }

        [Fact]
        public void TestTwoMonthTrainingRenewal()
        {
            var tr = _sf.InsertRequirement(months: 2);
            var expectedDates = new DateTime[2];
            _sf.InsertTraining(tr.Id, new DateTime(2017, StartMonth, 30).AddMonths(-1));
            _sf.InsertTraining(tr.Id, expectedDates[0] = new DateTime(2017, StartMonth, 1));
            _sf.InsertTraining(tr.Id, expectedDates[1] = new DateTime(2017, StartMonth, 30).AddMonths(1));
            _sf.InsertTraining(tr.Id, new DateTime(2017, StartMonth, 1).AddMonths(2));
            var staffTrainings = _repo.GetStaffTrainingByYearMonth(2017, StartMonth)
                .Select(training => training.CompletedDate.GetValueOrDefault().ToShortDateString()).ToArray();
            staffTrainings.Length.ShouldBe(2);
            staffTrainings.ShouldBeSubsetOf(expectedDates.Select(time => time.ToShortDateString()));
        }

        [Fact]
        public void TestSixMonthTrainingRenewalFirstHalfOfYear()
        {
            var tr = _sf.InsertRequirement(months: 6);
            var expectedDates = new DateTime[2];
            _sf.InsertTraining(tr.Id, new DateTime(2017, StartMonth, 30).AddMonths(-1));
            _sf.InsertTraining(tr.Id, expectedDates[0] = new DateTime(2017, StartMonth, 1));
            _sf.InsertTraining(tr.Id, expectedDates[1] = new DateTime(2017, StartMonth, 30).AddMonths(5));
            _sf.InsertTraining(tr.Id, new DateTime(2017, StartMonth, 1).AddMonths(6));
            var staffTrainings = _repo.GetStaffTrainingByYearMonth(2017, StartMonth)
                .Select(training => training.CompletedDate.GetValueOrDefault().ToShortDateString()).ToArray();
            staffTrainings.Length.ShouldBe(2);
            staffTrainings.ShouldBeSubsetOf(expectedDates.Select(time => time.ToShortDateString()));
        }

        [Fact]
        public void TestSixMonthTrainingRenewalInLastHalfOfYear()
        {
            var tr = _sf.InsertRequirement(months: 6);
            var expectedDates = new DateTime[2];
            _sf.InsertTraining(tr.Id, new DateTime(2018, EndMonth, 1).AddMonths(1));
            _sf.InsertTraining(tr.Id, expectedDates[0] = new DateTime(2018, EndMonth, 30));
            _sf.InsertTraining(tr.Id, expectedDates[1] = new DateTime(2018, EndMonth, 1).AddMonths(-5));
            _sf.InsertTraining(tr.Id, new DateTime(2018, EndMonth, 30).AddMonths(-6));
            var staffTrainings = _repo.GetStaffTrainingByYearMonth(2017, EndMonth - 2)
                .Select(training => training.CompletedDate.GetValueOrDefault().ToShortDateString()).ToArray();
            staffTrainings.Length.ShouldBe(2);
            staffTrainings.ShouldBeSubsetOf(expectedDates.Select(time => time.ToShortDateString()));
        }

        [Fact]
        public void TestTwentyFourMonthTrainingRenewal()
        {
            var tr = _sf.InsertRequirement(months: 24);
            _sf.InsertTraining(tr.Id, new DateTime(2016, StartMonth, 1));
            _sf.InsertTraining(tr.Id, new DateTime(2017, StartMonth, 1));
            _sf.InsertTraining(tr.Id, new DateTime(2017, StartMonth, 1));
        }

        [Fact]
        public void NegativeMonthsRequirementsAlwaysReturn()
        {
            var tr = _sf.InsertRequirement(months: -1);
            _sf.InsertTraining(tr.Id, new DateTime(2000, 3, 1));
            _sf.InsertTraining(tr.Id, new DateTime(2018, 3, 1));
            _sf.InsertTraining(tr.Id, new DateTime(2025, 3, 1));
            var training = _repo.GetStaffTrainingByYearMonth(2017, 3).ToArray();
            training.ShouldNotBeEmpty();
            training.Length.ShouldBe(3);
            training.Select(t => t.CompletedDate.GetValueOrDefault()).ShouldBe(new[]
            {
                new DateTime(2000, 3, 1),
                new DateTime(2018, 3, 1),
                new DateTime(2025, 3, 1)
            });
        }
    }
}