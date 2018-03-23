using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Services;
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

        [HttpGet("{id}")]
        public JobWithRoles GetById(Guid id)
        {
            return _jobService.GetById(id);
        }

        [HttpPost]
        public Job Save([FromBody] Job job)
        {
            _jobService.Save(job);
            return job;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _jobService.DeleteJob(id);
            return Ok();
        }
        
        

        [HttpGet("grade")]
        public IList<Grade> Grades()
        {
            return _jobService.JobGrades();
        }

        [HttpGet("grade/{id}")]
        public Grade GetGradeById(Guid id)
        {
            return _jobService.GetGradeById(id);
        }

        [HttpPost("grade")]
        public Grade Save([FromBody] Grade grade)
        {
            _jobService.Save(grade);
            return grade;
        }

        [HttpDelete("grade/{id}")]
        public IActionResult DeleteGrade(Guid id)
        {
            _jobService.DeleteGrade(id);
            return Ok();
        }
    }
}