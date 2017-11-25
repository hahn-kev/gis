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