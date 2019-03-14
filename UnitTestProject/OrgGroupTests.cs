using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using LinqToDB.Data;
using Shouldly;
using Xunit;

namespace UnitTestProject
{
    public class OrgGroupTests : IClassFixture<ServicesFixture>, IDisposable
    {
        private ServicesFixture _sf;
        private OrgGroupService _groupService;
        private OrgGroupRepository _groupRepository;
        private readonly DataConnectionTransaction _transaction;
        private PersonWithStaff org1Staff;
        private PersonWithStaff org1aStaff;
        private PersonWithStaff org1Super;
        private PersonWithStaff org2Super;
        private PersonWithStaff org1aSuper;
        private OrgGroup org1;
        private OrgGroup org1a;
        private OrgGroup orgRoot;

        private OrgGroup org2;
        private OrgGroup org2a;
        private PersonWithStaff org2aStaff;

        public OrgGroupTests(ServicesFixture sf)
        {
            _sf = sf;
            _groupService = _sf.Get<OrgGroupService>();
            _groupRepository = _sf.Get<OrgGroupRepository>();
            _transaction = _sf.DbConnection.BeginTransaction();
            var orgRootId = Guid.NewGuid();

            org1Super = _sf.InsertPerson();
            org1 = _sf.InsertOrgGroup(orgRootId, org1Super.Id, name: "org1", approvesLeave: true);

            org2Super = _sf.InsertPerson();
            org2 = _sf.InsertOrgGroup(orgRootId, org2Super.Id, name: "org2", approvesLeave: true);
            org2a = _sf.InsertOrgGroup(org2.Id, name: "org2a");

            org1aSuper = _sf.InsertPerson();
            org1a = _sf.InsertOrgGroup(org1.Id, org1aSuper.Id, name: "org1a");

            orgRoot = _sf.InsertOrgGroup(action: group => group.Id = orgRootId, name: "orgRoot");

            org2aStaff = _sf.InsertStaff(org2a.Id);
            org1Staff = _sf.InsertStaff(org1.Id);
            org1aStaff = _sf.InsertStaff(org1a.Id);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }

        public static IEnumerable<object[]> ValidateOrgGroupOrderData()
        {
            var g1 = new OrgGroup() {Id = Guid.NewGuid(), ParentId = Guid.NewGuid()};
            var g2 = new OrgGroup() {Id = g1.ParentId.Value, ParentId = Guid.NewGuid()};
            var g3 = new OrgGroup() {Id = g2.ParentId.Value, ParentId = Guid.NewGuid()};

            yield return new object[] {new List<OrgGroup> {g3, g2, g1}, OrgGroupService.SortedBy.Either, true};
            yield return new object[] {new List<OrgGroup> {g3, g2, g1}, OrgGroupService.SortedBy.ParentFirst, true};
            yield return new object[] {new List<OrgGroup> {g3, g2, g1}, OrgGroupService.SortedBy.ChildFirst, false};

            foreach (var sortedBy in Enum.GetValues(typeof(OrgGroupService.SortedBy))
                .OfType<OrgGroupService.SortedBy>())
            {
                yield return new object[] {new List<OrgGroup> {g2, g3, g1}, sortedBy, false};
                yield return new object[] {new List<OrgGroup> {g1, g3, g2}, sortedBy, false};
                yield return new object[] {new List<OrgGroup>(), sortedBy, true};
                yield return new object[] {new List<OrgGroup> {g1}, sortedBy, true};
            }

            yield return new object[] {new List<OrgGroup> {g1, g2, g3}, OrgGroupService.SortedBy.Either, true};
            yield return new object[] {new List<OrgGroup> {g1, g2, g3}, OrgGroupService.SortedBy.ParentFirst, false};
            yield return new object[] {new List<OrgGroup> {g1, g2, g3}, OrgGroupService.SortedBy.ChildFirst, true};
        }

        [Theory]
        [MemberData(nameof(ValidateOrgGroupOrderData))]
        public static void ShouldValidateOrderOfOrgGroups(List<OrgGroup> groups,
            OrgGroupService.SortedBy sortedBy,
            bool expected)
        {
            OrgGroupService.IsOrgGroupSortedByHierarchy(groups, sortedBy).ShouldBe(expected);
        }

        private void ShouldMatchOrder<T>(IEnumerable<T> actual, IEnumerable<T> expected)
        {
            var actualList = actual.ToList();
            actualList.ShouldSatisfyAllConditions(expected.Select<T, Action>((obj, i) =>
            {
                return () => actualList[i].ShouldBe(obj);
            }).ToArray());
        }

        [Fact]
        public void ShouldFailIfMatchDoesntFail()
        {
            Assert.ThrowsAny<Exception>(() => { ShouldMatchOrder(new[] {"1", "2", "3"}, new[] {"1", "3", "2"}); });
        }

        [Fact]
        public void ShouldGetParentsOrdered()
        {
            var orgGroups = _groupRepository.GetWithParentsWhere(group => @group.Id == org1a.Id)
                .Select(group => group.Id).ToList();
            ShouldMatchOrder(orgGroups,
                new[]
                {
                    org1a.Id,
                    org1.Id,
                    orgRoot.Id,
                });
        }

        [Fact]
        public void ShouldGetChildrenOrdered()
        {
            var orgGroups = _groupRepository.GetWithChildrenWhere(group => @group.Id == orgRoot.Id)
                .Select(group => group.GroupName).ToList();
            orgGroups.ShouldBe(new[]
            {
                orgRoot.GroupName,
                org1.GroupName,
                org2.GroupName,
                org1a.GroupName,
                org2a.GroupName
            });
        }

        [Fact]
        public async Task ShouldShowPersonInGroup()
        {
            (await _groupService.IsPersonInGroup(org1aStaff.Id, org1.Id)).ShouldBeTrue();
            (await _groupService.IsPersonInGroup(org1aStaff.Id, org2.Id)).ShouldBeFalse();
        }


        [Fact]
        public void OrgsByRolesResolvesAsExpected()
        {
            _sf.InsertRoleAndJob(org1aStaff.Id, jobOrgGroupId: org2a.Id);
            _sf.InsertRoleAndJob(org1aStaff.Id, jobOrgGroupId: org1a.Id);
            var orgs = _groupRepository.GetOrgGroupsByPersonsRole(org1aStaff.Id);

            orgs.ShouldContain(org => org.Id == org1a.Id);
            orgs.ShouldContain(org => org.Id == org2.Id);

            orgs.ShouldNotContain(org => org.Id == org2a.Id);
            orgs.ShouldNotContain(org => org.Id == org1.Id);
            orgs.ShouldNotContain(org => org.Id == orgRoot.Id);
            orgs.Count.ShouldBe(2);
        }

        [Fact]
        public void OrgsByReportingStaffResolvesAsExpected()
        {
            var reportingOrgs = _groupRepository.StaffParentOrgGroups(org1aStaff.Staff);
            reportingOrgs.ShouldContain(org => org.Id == org1a.Id);
            reportingOrgs.ShouldContain(org => org.Id == org1.Id);

            reportingOrgs.ShouldNotContain(org => org.Id == org2.Id);
            reportingOrgs.ShouldNotContain(org => org.Id == org2a.Id);
            reportingOrgs.ShouldNotContain(org => org.Id == orgRoot.Id);
            reportingOrgs.Count().ShouldBe(2);

            reportingOrgs = _groupRepository.StaffParentOrgGroups(org2aStaff.Staff);

            reportingOrgs.ShouldNotContain(org => org.Id == org2a.Id);
            reportingOrgs.ShouldNotContain(org => org.Id == org1.Id);
            reportingOrgs.ShouldNotContain(org => org.Id == org1a.Id);
            reportingOrgs.ShouldNotContain(org => org.Id == orgRoot.Id);
            reportingOrgs.ShouldHaveSingleItem().Id.ShouldBe(org2.Id);
        }
    }
}