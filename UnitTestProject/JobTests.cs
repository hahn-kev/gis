using System;
using System.Linq;
using Backend.Services;
using LinqToDB.Data;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class JobTests: IClassFixture<ServicesFixture>, IDisposable
    {
        private readonly ServicesFixture _sf;
        private readonly JobService _js;
        private readonly DataConnectionTransaction _transaction;

        public JobTests(ServicesFixture servicesFixture)
        {
            _sf = servicesFixture;
            _js = _sf.Get<JobService>();
            _transaction = _sf.DbConnection.BeginTransaction();
        }

        public void Dispose()
        {
            _transaction.Rollback();
        }

        [Fact]
        public void FetchJobListWithFilledDoesntThrow()
        {
            var job = _sf.InsertJob();
            _sf.InsertRole(job.Id, active: true);
            var jobs = _js.JobWithFilledInfos();
            jobs.ShouldNotBeNull();
            jobs.ShouldNotBeEmpty();
        }

        [Fact]
        public void FetchJobListWithFilledCountsActiveJobsOnly()
        {
            var job = _sf.InsertJob(j => j.Positions = 3);
            _sf.InsertRole(job.Id, active: true);
            _sf.InsertRole(job.Id, active: true);
            _sf.InsertRole(job.Id, active: false);
            var actualJob = _js.JobWithFilledInfos().Single();
            actualJob.Id.ShouldBe(job.Id);
            actualJob.Filled.ShouldBe(2);
            actualJob.Positions.ShouldBe(3);
        }

        [Fact]
        public void FetchJobsCountsRolesPerJob()
        {
            var job1 = _sf.InsertJob(j => j.Positions = 3);
            _sf.InsertRole(job1.Id, active: true);
            _sf.InsertRole(job1.Id, active: true);
            _sf.InsertRole(job1.Id, active: false);
            var job2 = _sf.InsertJob(j => j.Positions = 2);
            _sf.InsertRole(job2.Id, active: true);
            _sf.InsertRole(job2.Id, active: false);
            var jobs = _js.JobWithFilledInfos();
            var actualJob1 = jobs.SingleOrDefault(job => job.Id == job1.Id);
            actualJob1.ShouldNotBeNull();
            var actualJob2 = jobs.SingleOrDefault(job => job.Id == job2.Id);
            actualJob2.ShouldNotBeNull();
            actualJob1.Filled.ShouldBe(2);
            actualJob2.Filled.ShouldBe(1);
        }

        [Fact]
        public void FetchJobWithoutAnyRolesShouldHaveZeroFilled()
        {
            var job = _sf.InsertJob(j => j.Positions = 3);
            var actualJob = _js.JobWithFilledInfos().Single();
            actualJob.ShouldNotBeNull();
            actualJob.Filled.ShouldBe(0);
        }

        [Fact]
        public void FetchJobWithoutType()
        {
            var job1 = _sf.InsertJob(j => j.Type = null);
            _sf.InsertRole(job1.Id, active: true);
            var actualJob = _js.JobWithFilledInfos().Single();
            actualJob.Id.ShouldBe(job1.Id);
        }

        [Fact]
        public void FetchJobWitInactiveRole()
        {
            var job = _sf.InsertJob();
            _sf.InsertRole(job.Id, active: false);
            var jobs = _js.JobWithFilledInfos();
            jobs.ShouldNotBeNull();
            jobs.ShouldNotBeEmpty();
        }
    }
}