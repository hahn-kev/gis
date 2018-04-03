using System;
using System.Runtime.CompilerServices;

namespace Backend.Utils
{
    public struct EmailTemplate
    {
        private EmailTemplate(Func<TemplateSettings, string> idFunc, [CallerMemberName] string name = null)
        {
            _idFunc = idFunc;
            Name = name;
        }

        private readonly Func<TemplateSettings, string> _idFunc;

        public string GetId(TemplateSettings settings)
        {
            return _idFunc?.Invoke(settings) ??
                   throw new NullReferenceException("template " + Name + " id not found");
        }

        public string Name { get; }
        public static EmailTemplate NotifyLeaveRequest => new EmailTemplate(_ => _.NotifyLeaveRequest);
        public static EmailTemplate RequestLeaveApproval => new EmailTemplate(_ => _.RequestLeaveApproval);
        public static EmailTemplate NotifyHrLeaveRequest => new EmailTemplate(_ => _.NotifyHrLeaveRequest);

        public bool Equals(EmailTemplate other)
        {
            return String.Equals(Name, other.Name);
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
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}