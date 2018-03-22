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
        private readonly IEntityService _entityService;

        public OrgGroupService(OrgGroupRepository orgGroupRepository, IEntityService entityService)
        {
            _orgGroupRepository = orgGroupRepository;
            _entityService = entityService;
        }

        public List<OrgGroup> OrgGroups => _orgGroupRepository.OrgGroups.OrderBy(group => group.GroupName).ToList();

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
    }
}