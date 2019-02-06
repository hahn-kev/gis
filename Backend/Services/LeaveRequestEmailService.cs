using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Utils;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class LeaveRequestEmailService
    {
        private readonly IEmailService _emailService;
        private readonly PersonRepository _personRepository;
        private readonly Settings _settings;

        public LeaveRequestEmailService(IEmailService emailService,
            PersonRepository personRepository,
            IOptions<Settings> options)
        {
            _emailService = emailService;
            _personRepository = personRepository;
            _settings = options.Value;
        }

        public async Task NotifyOfLeaveRequest(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonWithStaff supervisor,
            PersonWithStaff approver,
            LeaveUsage leaveUsage)
        {
            var substitutions = BuildSubstitutions(leaveRequest, requestedBy, supervisor, approver, leaveUsage);
            await _emailService.SendTemplateEmail(substitutions,
                $"{PersonFullName(requestedBy)} has requested leave",
                EmailTemplate.NotifyLeaveRequest,
                requestedBy,
                supervisor);
        }

        public async Task NotifyHr(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonExtended supervisor,
            LeaveUsage leaveUsage)
        {
            if (!ShouldNotifyHr(leaveRequest, leaveUsage)) return;
            var substitutions = BuildSubstitutions(leaveRequest, requestedBy, supervisor, supervisor, leaveUsage);
            await _emailService.SendTemplateEmail(substitutions,
                $"{PersonFullName(requestedBy)} has requested leave",
                EmailTemplate.NotifyHrLeaveRequest,
                requestedBy,
                _personRepository.GetStaffNotifyHr());
        }

        public Task SendRequestApproval(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonWithStaff supervisor,
            LeaveUsage leaveUsage)
        {
            var substitutions = BuildSubstitutions(leaveRequest, requestedBy, supervisor, supervisor, leaveUsage);

            return _emailService.SendTemplateEmail(substitutions,
                $"{PersonFullName(requestedBy)} Leave request approval",
                EmailTemplate.RequestLeaveApproval,
                requestedBy,
                supervisor);
        }

        public Task NotifyRequestApproved(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonWithStaff approvedBy)
        {
            var substitutions = BuildSubstitutions(leaveRequest, requestedBy, approvedBy, approvedBy, null);
            return _emailService.SendTemplateEmail(substitutions,
                "Notify leave approved",
                EmailTemplate.NotifyLeaveApproved,
                approvedBy,
                requestedBy);
        }

        private Dictionary<string, string> BuildSubstitutions(LeaveRequest leaveRequest,
            Person requestedBy,
            Person supervisor,
            Person approver,
            LeaveUsage leaveUsage)
        {
            //this is a list of substitutions available in the email template
            //these are used for all leave requests
            //$LEAVE-SUBSTITUTIONS$
            var substitutions = new Dictionary<string, string>
            {
                {"type", leaveRequest.Type.ToString().SplitCamelCase()},
                {"approve", $"{_settings.BaseUrl}/api/leaveRequest/approve/{leaveRequest.Id}"},
                {"supervisorCalendarLink", $"{_settings.BaseUrl}/calendar/supervisor"},
                {"requestLink", $"{_settings.BaseUrl}/leave-request/edit/{leaveRequest.Id}"},
                {"supervisor", PersonFullName(supervisor)},
                {"approver", PersonFullName(approver)},
                {"requester", PersonFullName(requestedBy)},
                {"start", leaveRequest.StartDate.ToString("MMM d yyyy")},
                {"end", leaveRequest.EndDate.ToString("MMM d yyyy")},
                {"time", $"{leaveRequest.Days} Day{PluralSuffix(leaveRequest.Days)}"},
                {"reason", leaveRequest.Reason}
            };
            if (leaveUsage != null)
            {
                substitutions.Add("totalDays", $"{leaveUsage.TotalAllowed} Day{PluralSuffix(leaveUsage.TotalAllowed)}");
                substitutions.Add("left", $"{leaveUsage.Left} Day{PluralSuffix(leaveUsage.Left)}");
            }

            return substitutions;
        }


        public static bool ShouldNotifyHr(LeaveRequest leaveRequest, LeaveUsage leaveUsage)
        {
            if (leaveRequest.Type == LeaveType.Other) return true;
            return leaveUsage.Left < leaveRequest.Days;
        }

        private string PluralSuffix(decimal num)
        {
            return num == 1 ? string.Empty : "s";
        }

        private string PersonFullName(Person person)
        {
            return (person.PreferredName ?? person.FirstName) + " " + person.LastName;
        }
    }
}