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

        public MissionOrg GetOrg(Guid id)
        {
            return _dbConnection.MissionOrgs.SingleOrDefault(org => org.Id == id);
        }

        public IQueryable<MissionOrgWithNames> MissionOrgsWithNames =>
            from org in _dbConnection.MissionOrgs
            from person in _dbConnection.People.LeftJoin(person => person.Id == org.RepId)
            select new MissionOrgWithNames
            {
                Id = org.Id,
                Address = org.Address,
                ApprovedDate = org.ApprovedDate,
                Email = org.Email,
                Name = org.Name,
                OfficeInThailand = org.OfficeInThailand,
                Phone = org.Phone,
                RepId = org.RepId,
                Status = org.Status,
                ContactName = (person.PreferredName ?? person.FirstName) + " " + person.LastName
            };
    }
}