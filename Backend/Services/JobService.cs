using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;

namespace Backend.Services
{
    public class JobService
    {
        private readonly JobRepository _jobRepository;
        private readonly IEntityService _entityService;
        private readonly PersonRepository _personRepository;

        public JobService(JobRepository jobRepository, IEntityService entityService, PersonRepository personRepository)
        {
            _jobRepository = jobRepository;
            _entityService = entityService;
            _personRepository = personRepository;
        }

        public IList<Job> Jobs()
        {
            return _jobRepository.Job.OrderBy(job => job.Title).ToList();
        }

        public JobWithRoles GetById(Guid id)
        {
            return _jobRepository.GetById(id);
        }

        public void Save(Job job)
        {
            _entityService.Save(job);
        }

        public void Delete(Guid jobId)
        {
            _entityService.Delete<Job>(jobId);
        }
    }
}