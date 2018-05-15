using System;
using System.Linq;
using Backend.Entities;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace Backend.DataLayer
{
    public class MissionOrgRepository
    {
        private readonly IDbConnection _dbConnection;

        public MissionOrgRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public MissionOrgWithYearSummaries GetOrg(Guid id)
        {
            var missionOrg = _dbConnection.MissionOrgs.Select(org => new MissionOrgWithYearSummaries()
            {
                Id = org.Id,
                Name = org.Name,
                RepId = org.RepId,
                Phone = org.Phone,
                Email = org.Email,
                Address = org.Address,
                AddressLocal = org.AddressLocal,
                ApprovedDate = org.ApprovedDate,
                OfficeInThailand = org.OfficeInThailand,
                Status = org.Status
            }).SingleOrDefault(org => org.Id == id);
            if (missionOrg == null) throw new NullReferenceException("No Mission org found matching ID");
            missionOrg.YearSummaries = _dbConnection.MissionOrgYearSummaries
                .Where(year => year.MissionOrgId == id)
                .ToList();
            return missionOrg;
        }

        public IQueryable<MissionOrgWithNames> MissionOrgsWithNames =>
            from org in _dbConnection.MissionOrgs
            from person in _dbConnection.People.LeftJoin(person => person.Id == org.RepId)
            select new MissionOrgWithNames
            {
                Id = org.Id,
                Address = org.Address,
                AddressLocal = org.AddressLocal,
                ApprovedDate = org.ApprovedDate,
                Email = org.Email,
                Name = org.Name,
                OfficeInThailand = org.OfficeInThailand,
                Phone = org.Phone,
                RepId = org.RepId,
                Status = org.Status,
                ContactName = (person.PreferredName ?? person.FirstName) + " " + person.LastName
            };

        public IQueryable<Person> PeopleInOrg(Guid orgId)
        {
            return from p in _dbConnection.People
                from staff in _dbConnection.Staff.InnerJoin(staff => staff.Id == p.StaffId)
                from missionOrg in _dbConnection.MissionOrgs.InnerJoin(org => org.Id == staff.MissionOrgId)
                where missionOrg.Id == orgId
                select p;
        }
    }
}