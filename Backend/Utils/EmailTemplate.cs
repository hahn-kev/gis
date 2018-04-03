using System;
using System.Runtime.CompilerServices;

namespace Backend.Utils
{

    public struct EmailTemplate
    {
        private EmailTemplate(string id, [CallerMemberName] string name = null)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
        public static EmailTemplate NotifyLeaveRequest => new EmailTemplate("5aa3038a-6c0d-4e6c-bc57-311c87916a0c");
        public static EmailTemplate RequestLeaveApproval => new EmailTemplate("70b6165d-f367-401f-9ae4-56814033b720");
        public static EmailTemplate NotifyHrLeaveRequest => new EmailTemplate("14aa52db-f802-4e62-82db-6a3391bcf8a2");

        public bool Equals(EmailTemplate other)
        {
            return String.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EmailTemplate && Equals((EmailTemplate) obj);
        }

        public static bool operator ==(EmailTemplate a, EmailTemplate b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(EmailTemplate a, EmailTemplate b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}