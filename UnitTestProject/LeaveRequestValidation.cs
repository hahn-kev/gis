using System;
using AutoBogus;
using Backend.Entities;
using Backend.Services;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class LeaveRequestValidationTests
    {
        private LeaveRequest GenerateRequest(LeaveType? leaveType = null, bool? approved = null)
        {
            var leaveRequest = AutoFaker.Generate<LeaveRequest>();
            leaveRequest.Approved = approved;
            if (leaveRequest.StartDate > leaveRequest.EndDate)
            {
                var tmp = leaveRequest.StartDate;
                leaveRequest.StartDate = leaveRequest.EndDate;
                leaveRequest.EndDate = tmp;
            }

            if (leaveType.HasValue) leaveRequest.Type = leaveType.Value;

            leaveRequest.OverrideDays = false;
            leaveRequest.Days = leaveRequest.CalculateLength();
            return leaveRequest;
        }

        #region validation

        [Fact]
        public void ThrowsErrorWhenPersonIsInvalid()
        {
            LeaveRequest oldRequest = GenerateRequest();
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.PersonId = Guid.NewGuid();

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest));
            ex.Message.ShouldContain("change the person");
        }

        [Fact]
        public void ThrowsWhenChangingTheDays()
        {
            LeaveRequest oldRequest = GenerateRequest();
            LeaveRequest newRequest = oldRequest.Copy();

            newRequest.Days--;

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest));

            Assert.Contains("must match calculated", ex.Message);

            newRequest.Days = newRequest.CalculateLength();
            newRequest.EndDate += TimeSpan.FromDays(4);
            ex = Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest));

            Assert.Contains("must match calculated", ex.Message);
        }

        [Fact]
        public void ThrowsWhenChangingDaysWhenOverriden()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.OverrideDays = true;
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.Days++;

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest));

            Assert.Contains("modify the length", ex.Message);
        }

        [Fact]
        public void ThrowsWhenChangingStartAndEndForOverridenDays()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.OverrideDays = true;
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.EndDate += TimeSpan.FromDays(4);

            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest));

            Assert.Contains("modify the start or end", ex.Message);
        }

        [Fact]
        public void ThrowsIfChangingApproved()
        {
            LeaveRequest oldRequest = GenerateRequest();
            LeaveRequest newRequest = oldRequest.Copy();
            oldRequest.Approved = false;
            newRequest.Approved = true;
            var ex = Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest));
            Assert.Contains("approve", ex.Message);
        }

        [Fact]
        public void AcceptMissmatchedCalculationForHalfDays()
        {
            LeaveRequest oldRequest = GenerateRequest();
            //16th is a friday
            oldRequest.StartDate = new DateTime(2018, 3, 16);
            oldRequest.EndDate = new DateTime(2018, 3, 16);
            oldRequest.Days = oldRequest.CalculateLength();
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.Days = 0.5m;
            //does not throw
            LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest);
        }

        [Fact]
        public void DontMissmatchedCalculationForHalfDaysOnWeekend()
        {
            LeaveRequest oldRequest = GenerateRequest();
            //17th is a saturday
            oldRequest.StartDate = new DateTime(2018, 3, 17);
            oldRequest.EndDate = new DateTime(2018, 3, 17);
            oldRequest.Days = oldRequest.CalculateLength();
            LeaveRequest newRequest = oldRequest.Copy();
            newRequest.Days = 0.5m;
            //does not throw
            Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, newRequest));
        }

        [Fact]
        public void ThrowsForNewRequestsWhenOverridingDays()
        {
            LeaveRequest request = GenerateRequest();
            request.OverrideDays = true;
            Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(null, request));
        }

        [Fact]
        public void ThrowIfCalculationIsOff()
        {
            LeaveRequest request = GenerateRequest();

            //16th is a friday
            request.StartDate = new DateTime(2018, 3, 14);
            request.EndDate = new DateTime(2018, 3, 16);
            //should be 3 days
            request.Days = 2;
            Assert.Throws<ArgumentException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(null, request));
            request.Days.ShouldBe(2);
        }

        [Fact]
        public void ThrowsWhenModifyingApprovedLeave()
        {
            LeaveRequest oldRequest = GenerateRequest();
            oldRequest.Approved = true;
            Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, oldRequest));
            oldRequest.Approved = false;
            Assert.Throws<UnauthorizedAccessException>(() =>
                LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, oldRequest));
            oldRequest.Approved = null;
            //doesn't throw
            LeaveService.ThrowIfHrRequiredForUpdate(oldRequest, oldRequest);
        }

        #endregion
    }
}