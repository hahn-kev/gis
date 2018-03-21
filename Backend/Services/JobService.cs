using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;

namespace Backend.Services
{
    public class JobService
    {
        private JobRepository _jobRepository;
        private IEntityService _entityService;

        public JobService(JobRepository jobRepository, IEntityService entityService)
        {
            _jobRepository = jobRepository;
            _entityService = entityService;
        }

        public IList<Job> Jobs()
        {
            return _jobRepository.Job.OrderBy(job => job.Title).ToList();
        }

        public Job GetById(Guid id)
        {
            return _jobRepository.Job.SingleOrDefault(job => job.Id == id);
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