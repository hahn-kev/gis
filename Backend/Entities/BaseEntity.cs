using System;
using LinqToDB.Mapping;

namespace Backend.Entities
{
    public abstract class BaseEntity : IEquatable<BaseEntity>
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.Empty;

        public bool IsNew() => Id == Guid.Empty;

        public bool Equals(BaseEntity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseEntity) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(BaseEntity left, BaseEntity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseEntity left, BaseEntity right)
        {
            return !Equals(left, right);
        }
    }
}