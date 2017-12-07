using System;
using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class PersonRepository
    {
        private readonly DbConnection _dbConnection;

        public PersonRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Person> People => _dbConnection.People;
        public IQueryable<PersonExtended> PeopleExtended => _dbConnection.PeopleExtended;

        public IQueryable<PersonRoleExtended> PersonRolesExtended =>
            from personRole in _dbConnection.PersonRoles
            join person in People on personRole.PersonId equals person.Id
            select new PersonRoleExtended
            {
                Id = personRole.Id,
                PersonId = personRole.PersonId,
                Active = personRole.Active,
                Name = personRole.Name,
                StartDate = personRole.StartDate,
                EndDate = personRole.EndDate,
                FirstName = person.FirstName,
                LastName = person.LastName
            };

        public PersonExtended GetById(Guid id)
        {
            var personExtended = _dbConnection.PeopleExtended.FirstOrDefault(person => person.Id == id);
            if (personExtended != null)
            {
                personExtended.Roles = _dbConnection.PersonRoles.Where(role => role.PersonId == id).ToList();
            }
            return personExtended;
        }
    }
}