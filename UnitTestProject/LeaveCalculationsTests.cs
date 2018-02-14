using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using Backend.Services;
using Xunit;

namespace UnitTestProject
{
    public class LeaveCalculationsTests
    {
        public static IEnumerable<object[]> LeaveMemberData()
        {
            IEnumerable<(DateTime, DateTime, int)> MakeValues()
            {
                //May 1st 2015 is a Friday
                yield return (new DateTime(2015, 5, 1), new DateTime(2015, 5, 1), 1);
                //ensure time gets truncated
                yield return (new DateTime(2015, 5, 1), new DateTime(2015, 5, 1, 23, 59, 59), 1);
                //the second is a satruday, so it's not counted
                yield return (new DateTime(2015, 5, 1), new DateTime(2015, 5, 2), 1);
                //this spans the weekend, so we subtract 2 days
                yield return (new DateTime(2015, 5, 1), new DateTime(2015, 5, 7), 5);
                //this doesn't span a weekend
                yield return (new DateTime(2015, 5, 4), new DateTime(2015, 5, 5), 2);
                //gone for the whole week
                yield return (new DateTime(2015, 5, 4), new DateTime(2015, 5, 8), 5);
                //gone for 2 weeks
                yield return (new DateTime(2015, 5, 4), new DateTime(2015, 5, 15), 10);
                //gone for 2 weeks, but start date is a saturday, and end date is sunday
                yield return (new DateTime(2015, 5, 2), new DateTime(2015, 5, 17), 10);
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(LeaveMemberData))]
        public void ShouldMatchExpectedLeave(DateTime startDate, DateTime endDate, int expectedDays)
        {
            var result = PersonService.TotalLeaveUsed(new[] {new LeaveRequest(startDate, endDate)});
            Assert.Equal(expectedDays, result);
        }
    }
}