using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public abstract class BaseEntity
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.Empty;

        public bool IsNew() => Id == Guid.Empty;
    }
}