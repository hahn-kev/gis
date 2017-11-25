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
            return _dbConnection.PeopleExtended.FirstOrDefault(person => person.Id == id);
        }

        public void Update(PersonExtended person)
        {
            _dbConnection.Update(person);
        }

        public void Insert(PersonExtended person)
        {
            _dbConnection.Insert(person);
        }
    }
}