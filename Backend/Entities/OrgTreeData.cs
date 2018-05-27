using System.Collections.Generic;

namespace Backend.Entities
{
    public class OrgTreeData
    {
        public List<PersonRoleExtended> Roles { get; set; }
        public List<Job> Jobs { get; set; }
        public List<OrgGroupWithSupervisor> Groups { get; set; }
    }
}