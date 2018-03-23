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

        public IList<Grade> JobGrades()
        {
            return _jobRepository.JobGrades.OrderBy(grade => grade.GradeNo).ToList();
        }

        public JobWithRoles GetById(Guid id)
        {
            return _jobRepository.GetById(id);
        }

        public Grade GetGradeById(Guid id)
        {
            return _jobRepository.JobGrades.SingleOrDefault(grade => grade.Id == id);
        }

        public void Save(Job job)
        {
            _entityService.Save(job);
        }

        public void Save(Grade grade)
        {
            _entityService.Save(grade);
        }

        public void DeleteJob(Guid jobId)
        {
            _entityService.Delete<Job>(jobId);
        }

        public void DeleteGrade(Guid id)
        {
            _entityService.Delete<Grade>(id);
        }
    }
}