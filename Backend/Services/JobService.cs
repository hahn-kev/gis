﻿using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;

namespace Backend.Services
{
    public class JobService
    {
        private readonly JobRepository _jobRepository;
        private readonly IEntityService _entityService;

        public JobService(JobRepository jobRepository, IEntityService entityService)
        {
            _jobRepository = jobRepository;
            _entityService = entityService;
        }

        public List<Job> Jobs()
        {
            return _jobRepository.Job.ToList();
        }

        public IList<JobWithOrgGroup> JobsWithOrgGroup()
        {
            return _jobRepository.JobsWithOrgGroup.OrderBy(job => job.Title).ToList();
        }

        public IList<JobWithFilledInfo> JobWithFilledInfos()
        {
            return _jobRepository.JobWithFilledInfos.ToList();
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
            _jobRepository.PersonRoles.Where(role => role.JobId == jobId).Delete();
        }

        public void DeleteGrade(Guid id)
        {
            _entityService.Delete<Grade>(id);
        }

        public List<PersonRoleExtended> PersonRolesExtended => _jobRepository.PersonRolesExtended.ToList();
    }
}