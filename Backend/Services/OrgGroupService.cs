using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;

namespace Backend.Services
{
    public class OrgGroupService
    {
        private readonly OrgGroupRepository _orgGroupRepository;

        public OrgGroupService(OrgGroupRepository orgGroupRepository)
        {
            _orgGroupRepository = orgGroupRepository;
        }

        public List<OrgGroup> OrgGroups => _orgGroupRepository.OrgGroups.OrderBy(group => group.GroupName).ToList();

        public OrgGroup GetById(Guid id)
        {
            return _orgGroupRepository.OrgGroups.FirstOrDefault(group => group.Id == id);
        }
    }
}