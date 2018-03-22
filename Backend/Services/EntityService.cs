using System;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;

namespace Backend.Services
{
    public interface IEntityService
    {
        void Save<T>(T entity, out bool isNew) where T : BaseEntity;
        void Save<T>(T entity) where T : BaseEntity;
        void Delete<T>(T entity) where T : BaseEntity;
        void Delete<T>(Guid id) where T : BaseEntity;
    }

    public class EntityService : IEntityService
    {
        private readonly IDbConnection _dbConnection;

        public EntityService(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void Save<T>(T entity) where T : BaseEntity
        {
            Save(entity, out var _);
        }

        public void Save<T>(T entity, out bool isNew) where T : BaseEntity
        {
            isNew = false;
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
                _dbConnection.Insert(entity);
                isNew = true;
                return;
            }

            if (_dbConnection.Update(entity) != 1)
                throw new Exception($"{typeof(T).Name} not found for id: {entity.Id}");
        }

        public void Delete<T>(T entity) where T : BaseEntity
        {
            if (entity.Id == Guid.Empty) return;
            _dbConnection.Delete(entity);
            _dbConnection.Attachments.Where(attachment => attachment.AttachedToId == entity.Id).Delete();
        }

        public void Delete<T>(Guid id) where T : BaseEntity
        {
            _dbConnection.GetTable<T>().Delete(arg => arg.Id == id);
            _dbConnection.Attachments.Where(attachment => attachment.AttachedToId == id).Delete();
        }
    }
}