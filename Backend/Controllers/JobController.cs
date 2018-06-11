using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class JobController : MyController
    {
        private readonly JobService _jobService;

        public JobController(JobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public IList<Job> Jobs()
        {
            return _jobService.Jobs();
        }

        [HttpGet("orgGroups")]
        public IList<JobWithOrgGroup> JobsWithOrgGroup()
        {
            return _jobService.JobsWithOrgGroup();
        }

        [HttpGet("filled")]
        [Authorize(Policy = "jobs")]
        public IList<JobWithFilledInfo> JobsFilled()
        {
            return _jobService.JobWithFilledInfos();
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "jobs")]
        public JobWithRoles GetById(Guid id)
        {
            return _jobService.GetById(id);
        }

        [HttpPost]
        [Authorize(Policy = "jobs")]
        public Job Save([FromBody] Job job)
        {
            _jobService.Save(job);
            return job;
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "jobs")]
        public IActionResult Delete(Guid id)
        {
            _jobService.DeleteJob(id);
            return Ok();
        }


        [HttpGet("grade")]
        [Authorize(Policy = "grades")]
        public IList<Grade> Grades()
        {
            return _jobService.JobGrades();
        }

        [HttpGet("grade/{id}")]
        [Authorize(Policy = "grades")]
        public Grade GetGradeById(Guid id)
        {
            return _jobService.GetGradeById(id);
        }

        [HttpPost("grade")]
        [Authorize(Policy = "grades")]
        public Grade Save([FromBody] Grade grade)
        {
            _jobService.Save(grade);
            return grade;
        }

        [HttpDelete("grade/{id}")]
        [Authorize(Policy = "grades")]
        public IActionResult DeleteGrade(Guid id)
        {
            _jobService.DeleteGrade(id);
            return Ok();
        }

        [HttpGet("roles/all")]
        [Authorize(Policy = "role")]
        public IList<PersonRoleExtended> AllRoles()
        {
            return _jobService.PersonRolesExtended;
        }
    }
}