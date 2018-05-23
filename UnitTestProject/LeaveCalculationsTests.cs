﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Backend.Entities;
using Backend.Services;
using Backend.Utils;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class LeaveCalculationsTests
    {
        private LeaveService _leaveService;
        private ServicesFixture _servicesFixture;

        public LeaveCalculationsTests()
        {
            _servicesFixture = new ServicesFixture();
            _servicesFixture.SetupData();
            _leaveService = _servicesFixture.Get<LeaveService>();
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
            leaveRequest.Days = leaveRequest.CalculateLength();
            var result = LeaveService.TotalLeaveUsed(new[] {leaveRequest});
            Assert.Equal(expectedDays, result);
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
            var personWithStaff = _servicesFixture.InsertPerson(person => person.IsThai = true);
            var job = _servicesFixture.InsertJob();
            var expectedDays = 5;
            var leaveRequest = _servicesFixture.InsertLeaveRequest(LeaveType.Other, personWithStaff.Id, expectedDays);

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

            IEnumerable<(int?, IList<PersonRoleWithJob> )> MakeValues()
            {
                var twoYearsAgo = DateTime.Now - TimeSpan.FromDays(2 * 366);
                yield return (10,
                    new List<PersonRoleWithJob>
                    {
                        Role(startDate: twoYearsAgo)
                    });
                yield return (10,
                    new List<PersonRoleWithJob>
                    {
                        Role(startDate: twoYearsAgo),
                        //director position doesn't count to the 20 days because it's not active
                        Role(false, new DateTime(2000, 1, 1), new DateTime(2002, 1, 1),
                            supervisorId: personId)
                    });
                yield return (20, new List<PersonRoleWithJob>
                {
                    Role(startDate: twoYearsAgo, supervisorId: personId)
                });
                //don't get extra leave if supervisor isn't me
                yield return (10, new List<PersonRoleWithJob>
                {
                    Role(startDate: twoYearsAgo, supervisorId: Guid.NewGuid())
                });
                yield return (15, new List<PersonRoleWithJob>
                {
                    Role(startDate: twoYearsAgo),
                    Role(false, new DateTime(2000, 1, 1), new DateTime(2002, 1, 1)),
                    Role(false, new DateTime(2002, 1, 2), new DateTime(2013, 1, 1))
                });
                yield return (20, new List<PersonRoleWithJob>
                {
                    Role(startDate: twoYearsAgo),
                    Role(false, new DateTime(2000, 1, 1), new DateTime(2002, 1, 1)),
                    Role(false, new DateTime(2002, 1, 2), new DateTime(2013, 1, 1)),
                    Role(false, new DateTime(1995, 1, 1), new DateTime(2000, 1, 1)),
                });
                yield return (0, new List<PersonRoleWithJob>());
                //non fulltime/half don't get leave
                yield return (0, new List<PersonRoleWithJob>
                {
                    Role(startDate: twoYearsAgo, jobType: JobStatus.Contractor),
                    Role(startDate: twoYearsAgo, jobType: JobStatus.DailyWorker),
                    Role(startDate: twoYearsAgo, jobType: JobStatus.SchoolAid)
                });
            }

            return MakeValues().Select(tuple => tuple.ToArray());
        }

        [Theory]
        [MemberData(nameof(RolesMemberData))]
        public void ShouldCalculateLeaveAllowed(int expected, IList<PersonRoleWithJob> personRoles)
        {
            var result = LeaveService.LeaveAllowed(LeaveType.Vacation, personRoles);
            Assert.Equal(expected, result);
        }


        [Fact]
        public void ListAllLeaveWorks()
        {
            var personAndLeaveDetailses = _leaveService.PeopleWithLeave(null);
            Assert.NotEmpty(personAndLeaveDetailses);
        }
    }
}