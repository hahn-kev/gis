using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Backend.Utils;
using LinqToDB;
using LinqToDB.Data;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class LeaveCalculationsTests : IClassFixture<ServicesFixture>, IDisposable
    {
        private LeaveService _leaveService;
        private ServicesFixture _sf;
        private DataConnectionTransaction _transaction;

        public LeaveCalculationsTests(ServicesFixture servicesFixture)
        {
            _sf = servicesFixture;
            _sf.DoOnce(f => f.SetupData());
            _transaction = _sf.DbConnection.BeginTransaction();
            _leaveService = _sf.Get<LeaveService>();
        }


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
                //the 3rd is a sunday so it's not counted
                yield return (new DateTime(2015, 5, 1), new DateTime(2015, 5, 3), 1);
                //the 4th is monday, so it's counted
                yield return (new DateTime(2015, 5, 1), new DateTime(2015, 5, 4), 2);
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
            var leaveRequest = new LeaveRequest(startDate, endDate);
            leaveRequest.Days = LeaveService.CalculateLeaveDays(leaveRequest, new List<Holiday>());
            var result = LeaveService.TotalLeaveUsed(new[] {leaveRequest});
            Assert.Equal(expectedDays, result);
        }

        public static IEnumerable<object[]> LeaveHolidayData()
        {
            LeaveRequest LR(DateTime start, DateTime end) => new LeaveRequest {StartDate = start, EndDate = end};

            Holiday H(DateTime start, DateTime end) => new Holiday {Start = start, End = end};

            IEnumerable<(LeaveRequest, Holiday, int)> MakeValues()
            {
                //the 4th is a Monday
                //gone for whole week, but Tuesday through Thursday is a holiday
                yield return (
                    LR(new DateTime(2015, 5, 4), new DateTime(2015, 5, 8)),
                    H(new DateTime(2015, 5, 5), new DateTime(2015, 5, 7)),
                    2);

                //gone Tuesday through Thursday but the whole week is a holiday
                yield return (
                    LR(new DateTime(2015, 5, 5), new DateTime(2015, 5, 7)),
                    H(new DateTime(2015, 5, 4), new DateTime(2015, 5, 8)),
                    0);

                //gone Monday which is also a holiday
                yield return (
                    LR(new DateTime(2015, 5, 4), new DateTime(2015, 5, 4)),
                    H(new DateTime(2015, 5, 4), new DateTime(2015, 5, 4)),
                    0);
                //gone Monday - Tuesday, Monday is a holiday
                yield return (
                    LR(new DateTime(2015, 5, 4), new DateTime(2015, 5, 5)),
                    H(new DateTime(2015, 5, 4), new DateTime(2015, 5, 4)),
                    1);
                //gone Tuesday, but holiday is Monday - Wednesday which is also a holiday
                yield return (
                    LR(new DateTime(2015, 5, 5), new DateTime(2015, 5, 5)),
                    H(new DateTime(2015, 5, 4), new DateTime(2015, 5, 6)),
                    0);

                //gone Monday, Tuesday but holiday is Tuesday Wednesday
                yield return (
                    LR(new DateTime(2015, 5, 4), new DateTime(2015, 5, 5)),
                    H(new DateTime(2015, 5, 5), new DateTime(2015, 5, 6)),
                    1);
                //gone Tuesday, Wednesday but holiday is Monday, Tuesday
                yield return (
                    LR(new DateTime(2015, 5, 5), new DateTime(2015, 5, 6)),
                    H(new DateTime(2015, 5, 4), new DateTime(2015, 5, 5)),
                    1);

                //no overlap at all
                yield return (
                    LR(new DateTime(2015, 5, 5), new DateTime(2015, 5, 6)),
                    H(new DateTime(2015, 5, 7), new DateTime(2015, 5, 8)),
                    2);
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(LeaveHolidayData))]
        public void ShouldWorkProperlyWithHolidays(LeaveRequest leaveRequest, Holiday holiday, int expectedDays)
        {
            LeaveService.CalculateLeaveDays(leaveRequest, new List<Holiday> {holiday}).ShouldBe(expectedDays);
        }

        [Fact]
        public void ShouldNotCountRejectedLeaveRequests()
        {
            var pendingRequest = new LeaveRequest(new DateTime(2015, 5, 4), new DateTime(2015, 5, 5))
            {
                Approved = null
            };
            pendingRequest.Days.ShouldBe(2);
            var approvedRequest = new LeaveRequest(new DateTime(2015, 5, 4), new DateTime(2015, 5, 4))
            {
                Approved = true
            };
            approvedRequest.Days.ShouldBe(1);
            var rejectedRequest = new LeaveRequest(new DateTime(2015, 5, 4), new DateTime(2015, 5, 7))
            {
                Approved = false
            };
            rejectedRequest.Days.ShouldBe(4);
            LeaveService.TotalLeaveUsed(new[] {pendingRequest, approvedRequest, rejectedRequest}).ShouldBe(3);
        }

        [Fact]
        public void ShouldAllowOverridingLeaveUsed()
        {
            var startDate = new DateTime(2015, 5, 4);
            var endDate = new DateTime(2015, 5, 5);

            var result =
                LeaveService.TotalLeaveUsed(new[]
                {
                    new LeaveRequest(startDate, endDate)
                    {
                        OverrideDays = true,
                        Days = 5
                    }
                });
            Assert.Equal(2, startDate.BusinessDaysUntil(endDate));
            Assert.Equal(5, result);
        }

        [Fact]
        public void LeaveDetailsShouldIncludeOtherLeaveTypes()
        {
            var personWithStaff = _sf.InsertPerson(person => person.IsThai = true);
            var job = _sf.InsertJob();
            var expectedDays = 5;
            var leaveRequest = _sf.InsertLeaveRequest(LeaveType.Other, personWithStaff.Id, expectedDays);

            var currentLeaveDetails = _leaveService.GetCurrentLeaveDetails(personWithStaff.Id);
            var leaveUseage =
                currentLeaveDetails.LeaveUsages.SingleOrDefault(useage => useage.LeaveType == LeaveType.Other);
            Assert.NotNull(leaveUseage);
            Assert.Equal(expectedDays, leaveUseage.Used);
        }

        public static IEnumerable<object[]> RolesMemberData()
        {
            Guid personId = Guid.NewGuid();

            PersonRoleWithJob Role(bool active = true,
                DateTime startDate = default(DateTime),
                DateTime? endDate = null,
                JobStatus jobType = JobStatus.FullTime,
                Guid? supervisorId = null,
                GroupType groupType = GroupType.Division)
            {
                return new PersonRoleWithJob
                {
                    PersonId = personId,
                    Active = active,
                    Job = new JobWithOrgGroup
                    {
                        Status = jobType,
                        OrgGroup = new OrgGroup {Supervisor = supervisorId ?? Guid.NewGuid(), Type = groupType}
                    },
                    StartDate = startDate,
                    EndDate = endDate
                };
            }

            IEnumerable<(int, PersonRoleWithJob[])> MakeValues()
            {
                var twoYearsAgo = new DateTime(2018, 2, 22) - TimeSpan.FromDays(2 * 366);
                yield return (10,
                    new[]
                    {
                        Role(startDate: twoYearsAgo)
                    });
                yield return (10,
                    new[]
                    {
                        Role(startDate: twoYearsAgo),
                        //director position doesn't count to the 20 days because it's not active
                        Role(false,
                            new DateTime(2000, 1, 1),
                            new DateTime(2002, 1, 1),
                            supervisorId: personId)
                    });
                yield return (20, new[]
                {
                    Role(startDate: twoYearsAgo, supervisorId: personId)
                });
                //don't get extra leave if supervisor isn't me
                yield return (10, new[]
                {
                    Role(startDate: twoYearsAgo, supervisorId: Guid.NewGuid())
                });
                yield return (15, new[]
                {
                    Role(startDate: twoYearsAgo),
                    Role(false, new DateTime(2000, 1, 1), new DateTime(2002, 1, 1)),
                    Role(false, new DateTime(2002, 1, 2), new DateTime(2013, 1, 1))
                });
                yield return (20, new[]
                {
                    Role(startDate: twoYearsAgo),
                    Role(false, new DateTime(2000, 1, 1), new DateTime(2002, 1, 1)),
                    Role(false, new DateTime(2002, 1, 2), new DateTime(2013, 1, 1)),
                    Role(false, new DateTime(1995, 1, 1), new DateTime(2000, 1, 1)),
                });
                yield return (0, new PersonRoleWithJob[0]);
                //non fulltime/half don't get leave
                yield return (0, new[]
                {
                    Role(startDate: twoYearsAgo, jobType: JobStatus.Contractor),
                    Role(startDate: twoYearsAgo, jobType: JobStatus.DailyWorker),
                    Role(startDate: twoYearsAgo, jobType: JobStatus.SchoolAid)
                });

                //10 month jobs don't get leave now
                yield return (0, new[]
                {
                    Role(startDate: twoYearsAgo, jobType: JobStatus.FullTime10Mo)
                });
                //10 month jobs do count for leave time if you're not in a 10 month job now
                yield return (15, new[]
                {
                    Role(false, new DateTime(2002, 1, 2), new DateTime(2013, 1, 1), JobStatus.FullTime10Mo),
                    Role(startDate: twoYearsAgo)
                });
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(RolesMemberData))]
        public void ShouldCalculateLeaveAllowed(int expected, PersonRoleWithJob[] personRoles)
        {
            var result =
                LeaveService.LeaveAllowed(LeaveType.Vacation, personRoles, new DateTime(2018, 2, 21).SchoolYear());
            Assert.Equal(expected, result);
        }


        [Fact]
        public void ListAllLeaveWorks()
        {
            var personAndLeaveDetailses = _leaveService.PeopleWithCurrentLeave();
            Assert.NotEmpty(personAndLeaveDetailses);
        }


        [Fact]
        public void LeaveListByOrgGroupShouldIncludeChildGroupStaff()
        {
            PersonWithStaff rootStaff = null;

            PersonWithStaff aStaff = null;
            PersonWithStaff bStaff = null;
            PersonWithStaff a1Staff = null;
            var rootGroup = _sf.InsertOrgGroup(action: rootGroupA =>
            {
                rootStaff = _sf.InsertStaff(rootGroupA.Id);
                _sf.InsertOrgGroup(rootGroupA.Id,
                    action: aGroupA =>
                    {
                        aStaff = _sf.InsertStaff(aGroupA.Id);
                        _sf.InsertOrgGroup(aGroupA.Id,
                            action: a1GroupA => { a1Staff = _sf.InsertStaff(a1GroupA.Id); });
                    });

                _sf.InsertOrgGroup(rootGroupA.Id,
                    action: bGroup => bStaff = _sf.InsertStaff(bGroup.Id));
            });
            rootStaff.ShouldNotBeNull();
            aStaff.ShouldNotBeNull();
            bStaff.ShouldNotBeNull();
            a1Staff.ShouldNotBeNull();
            aStaff.Staff.OrgGroupId.ShouldNotBeNull();
            var actualStaff =
                _leaveService.PeopleInGroupWithLeave(aStaff.Staff.OrgGroupId.Value, DateTime.Now.SchoolYear());
            actualStaff.Select(details => details.Person.Id).ShouldBe(new[] {aStaff.Id, a1Staff.Id}, true);
            actualStaff.Select(details => details.Person.Id).ShouldNotContain(rootStaff.Id);
            actualStaff.Select(details => details.Person.Id).ShouldNotContain(bStaff.Id);
        }

        static LeaveUsage VacationLeave(IEnumerable<LeaveUsage> enumerable)
        {
            return enumerable.Single(usage => usage.LeaveType == LeaveType.Vacation);
        }

        static LeaveUsage[] WaysToGetVacationLeaveCalculation(Guid personId, int year, LeaveService leaveService)
        {
            return new[]
            {
                VacationLeave(leaveService.GetLeaveDetails(personId, year).LeaveUsages),
                VacationLeave(leaveService.PeopleWithLeave(year)
                    .Single(details => details.Person.Id == personId).LeaveUsages),
                VacationLeave(leaveService.PersonWithLeave(personId, year).LeaveUsages),
                leaveService.GetLeaveUseage(LeaveType.Vacation, personId, year),
            };
        }

        [Fact]
        public void EnsureDifferentWaysOfCalculatingLeaveMatchExpected()
        {
            var person = _sf.InsertPerson(staff => staff.IsThai = false);

            void InsertLeaveRequest(DateTime start, DateTime? end = null, bool? approved = true)
            {
                var leaveRequest = new LeaveRequest
                {
                    Id = Guid.NewGuid(),
                    PersonId = person.Id,
                    Type = LeaveType.Vacation,
                    StartDate = start,
                    EndDate = end ?? start,
                    Approved = approved
                };
                leaveRequest.Days = leaveRequest.CalculateLength();
                _sf.DbConnection.Insert(leaveRequest);
            }

            InsertLeaveRequest(new DateTime(2018, 4, 30));
            InsertLeaveRequest(new DateTime(2018, 4, 30), new DateTime(2018, 4, 30), false);
            InsertLeaveRequest(new DateTime(2018, 4, 30), new DateTime(2018, 4, 30), null);
            InsertLeaveRequest(new DateTime(2018, 6, 25), new DateTime(2018, 6, 29));

            WaysToGetVacationLeaveCalculation(person.Id, 2017, _leaveService).ShouldAllBe(usage => usage.Used == 7);
        }

        public static IEnumerable<object[]> GetEnsureAllLeaveUseTheSameDateRanges()
        {
            var personFaker = ServicesFixture.PersonFaker();
            var rangeStart = new DateTime(2018, 5, 25);
            var rangeEnd = new DateTime(2018, 8, 5);
            for (int j = 1; j < 4; j++)
            {
                var requestDate = rangeStart;
                do
                {
                    var requests = new LeaveRequest[j];
                    var person = personFaker.Generate();
                    person.IsThai = false;
                    //insert 3 leave requests and test those 3 at a time
                    for (int i = 0; i < j; i++)
                    {
                        var leaveRequest = new LeaveRequest
                        {
                            Id = Guid.NewGuid(),
                            Approved = true,
                            StartDate = requestDate,
                            EndDate = requestDate,
                            PersonId = person.Id,
                            Type = LeaveType.Vacation
                        };
                        leaveRequest.Days = leaveRequest.CalculateLength();
                        requests[i] = leaveRequest;
                        requestDate += TimeSpan.FromDays(1);
                    }

                    yield return new object[] {person, requests, j};
                } while (requestDate < rangeEnd);
            }
        }

        [Theory]
        [MemberData(nameof(GetEnsureAllLeaveUseTheSameDateRanges))]
        public void EnsureAllLeaveCalculatesUseTheSameDateRanges(PersonWithStaff person,
            ICollection<LeaveRequest> requests,
            int size)
        {
            _sf.DbConnection.Insert(person);
            _sf.DbConnection.Insert<Staff>(person.Staff);
            _sf.DbConnection.BulkCopy(requests);

            var leaveUsages =
                WaysToGetVacationLeaveCalculation(person.Id, 2017, _sf.Get<LeaveService>());
            leaveUsages.ShouldAllBe(used => leaveUsages.First().Used == used.Used,
                () =>
                    $"Window at {requests.First().StartDate.ToShortDateString()} Size: {size}, Requests: [{string.Join(", ", requests)}]");
        }

        public static IEnumerable<object[]> GetExpectedLeaveValues()
        {
            Guid personId = Guid.Empty;

            PersonWithStaff P()
            {
                Guid staffId = Guid.NewGuid();
                personId = Guid.NewGuid();
                return new PersonWithStaff
                {
                    IsThai = true,
                    Id = personId,
                    StaffId = staffId,
                    Staff = new StaffWithOrgName {Id = staffId}
                };
            }

            DateTime Date(int year, int month, int day)
            {
                return new DateTime(year, month, day);
            }

            LeaveRequest LR(DateTime start, DateTime end)
            {
                return new LeaveRequest(start, end)
                {
                    Approved = true,
                    Id = Guid.NewGuid(),
                    PersonId = personId,
                    Type = LeaveType.Vacation
                };
            }

            IEnumerable<(PersonWithStaff, LeaveRequest[], int used)> MakeValues()
            {
                //NOTE dates are based on 2018, which is the 2017 school year,
                //any dates after school is out should be the 2018 school year
                yield return (P(),
                    new[]
                    {
                        LR(Date(2018, 4, 5), Date(2018, 4, 6))
                    }, used: 2);

                yield return (P(),
                    new[]
                    {
                        LR(Date(2018, 4, 5), Date(2018, 4, 6)),
                        LR(Date(2018, 5, 30), Date(2018, 6, 1))
                    }, used: 5);

                yield return (P(),
                    new[]
                    {
                        LR(Date(2018, 6, 15), Date(2018, 6, 15)),
                        LR(Date(2017, 6, 15), Date(2017, 6, 15))
                    }, used: 1);

                yield return (P(),
                    new[]
                    {
                        LR(Date(2017, 7, 19), Date(2017, 7, 19))
                    }, used: 1);

                yield return (P(),
                    new[]
                    {
                        //first week day of July 2017
                        LR(Date(2017, 7, 3), Date(2017, 7, 3))
                    }, used: 1);

                yield return (P(),
                    new[]
                    {
                        //last week day of June 2017
                        LR(Date(2017, 6, 30), Date(2017, 6, 30))
                    }, used: 0);

                yield return (P(),
                    new[]
                    {
                        LR(Date(2017, 8, 8), Date(2017, 8, 8))
                    }, used: 1);


//                var rangeStart = new DateTime(2018, 5, 25);
//                var rangeEnd = new DateTime(2018, 8, 5);
//                var requestDate = rangeStart;
//                do
//                {
//                    var person = P();
//                    var lr18 = LR(requestDate, requestDate);
//                    var lr17 = LR(requestDate.AddYears(-1), requestDate.AddYears(-1));
//                    //exclude when they're on weekends
//                    if (lr18.Days == 1 && lr17.Days == 1)
//                        yield return (person, new[] {lr18, lr17}, used: 1);
//                    requestDate += TimeSpan.FromDays(1);
//                } while (requestDate < rangeEnd);
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(GetExpectedLeaveValues))]
        public void LeaveUsedIsCalculatedAsExpected(PersonWithStaff person, LeaveRequest[] leaveRequests, int used)
        {
            _sf.DbConnection.Insert(person);
            _sf.DbConnection.Insert(person.Staff);
            _sf.DbConnection.BulkCopy(leaveRequests);

            WaysToGetVacationLeaveCalculation(person.Id, 2017, _leaveService).ShouldAllBe(usage => usage.Used == used);
        }


        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}