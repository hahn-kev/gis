using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;

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
            var data = new OrgTreeData
            {
                Groups = OrgGroups
            };
            if (groupId.HasValue)
            {
                var orgIds = new HashSet<Guid>(new[] {groupId.Value});
                int oldCount;
                do
                {
                    oldCount = orgIds.Count;
                    orgIds.UnionWith(data.Groups
                        .Where(org => org.ParentId.HasValue && orgIds.Contains(org.ParentId.Value))
                        .Select(org => org.Id));
                } while (oldCount < orgIds.Count);

                data.Groups = data.Groups.FindAll(org => orgIds.Contains(org.Id));
                data.Jobs = _jobRepository.Job
                    .Where(job => orgIds.Contains(job.OrgGroupId)).ToList();
                var jobIds = data.Jobs.Select(job => job.Id).ToList();
                data.Roles = _jobRepository.PersonRolesExtended
                    .Where(role => jobIds.Contains(role.JobId)).ToList();
            }
            else
            {
                data.Jobs = _jobRepository.Job.ToList();
                data.Roles = _jobRepository.PersonRolesExtended.ToList();
            }

            return data;
        }
    }
}