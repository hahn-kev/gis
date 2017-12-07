﻿using System;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;

namespace Backend.Services
{
    public interface IEntityService
    {
        void Save<T>(T entity) where T : BaseEntity;
    }

    public class EntityService : IEntityService
    {
        private readonly DbConnection _dbConnection;

        public EntityService(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public void Save<T>(T entity) where T : BaseEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
                _dbConnection.Insert(entity);
                return;
            }
            if (_dbConnection.Update(entity) != 1)
                throw new Exception($"{typeof(T).Name} not found for id: {entity.Id}");
        }
    }
}