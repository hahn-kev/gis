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
        public Job GetById(Guid id)
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
            _jobService.Delete(id);
            return Ok();
        }
    }
}