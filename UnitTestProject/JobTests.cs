using System.Linq;
using Backend.Services;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class JobTests
    {
        private ServicesFixture _sf;
        private JobService _js;

        public JobTests()
        {
            _sf = new ServicesFixture();
            _js = _sf.Get<JobService>();
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
    }
}