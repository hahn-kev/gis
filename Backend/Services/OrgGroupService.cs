using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;
using LinqToDB.Tools;

namespace Backend.Services
{
    public class OrgGroupService
    {
        private readonly OrgGroupRepository _orgGroupRepository;
        private readonly JobRepository _jobRepository;
        private readonly IEntityService _entityService;

        public OrgGroupService(OrgGroupRepository orgGroupRepository,
            IEntityService entityService,
            JobRepository jobRepository)
        {
            _orgGroupRepository = orgGroupRepository;
            _entityService = entityService;
            _jobRepository = jobRepository;
        }

        public List<OrgGroupWithSupervisor> OrgGroups =>
            _orgGroupRepository.OrgGroupsWithSupervisor.OrderBy(group => group.GroupName).ToList();

        public OrgGroup GetById(Guid id)
        {
            return _orgGroupRepository.OrgGroups.FirstOrDefault(group => group.Id == id);
        }

        public void Save(OrgGroup orgGroup)
        {
            _entityService.Save(orgGroup);
        }

        public void Delete(Guid id)
        {
            _orgGroupRepository.OrgGroups.Where(child => child.ParentId == id)
                .Set(child => child.ParentId,
                    () => _orgGroupRepository.OrgGroups.Where(group => group.Id == id).Select(group => group.ParentId)
                        .SingleOrDefault()).Update();
            _entityService.Delete<OrgGroup>(id);
        }

        public OrgTreeData OrgTreeData(Guid? groupId = null)
        {
            var orgGroups = groupId.HasValue
                ? _orgGroupRepository.GetByIdWithChildren(groupId.Value)
                : _orgGroupRepository.OrgGroups;

            var jobs = from job in _jobRepository.Job
                from orgGroup in orgGroups.InnerJoin(g => g.Id == job.OrgGroupId)
                select job;
            var data = new OrgTreeData
            {
                Groups = (from orgGroup in _orgGroupRepository.OrgGroupsWithSupervisor
                    where orgGroup.Id.In(orgGroups.Select(g => g.Id))
                    select orgGroup).ToList(),
                Jobs = jobs.ToList(),
                Roles = (from role in _jobRepository.PersonRolesExtended
                    from job in jobs.InnerJoin(job => job.Id == role.JobId)
                    select role).ToList()
            };

            return data;
        }
    }
}