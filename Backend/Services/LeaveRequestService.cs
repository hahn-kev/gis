using System;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;

namespace Backend.Services
{
    public class LeaveRequestService
    {
        private readonly OrgGroupRepository _orgGroupRepository;
        private readonly PersonRepository _personRepository;
//        private readonly EmailService _emailService;

        public Person RequestLeave(LeaveRequest leaveRequest)
        {
            var result =
            (from personOnLeave in _personRepository.People.Where(person => person.Id == leaveRequest.PersonId)
                from department in _orgGroupRepository.OrgGroups.Where(@group => @group.Id == personOnLeave.OrgGroupId)
                from devision in _orgGroupRepository.OrgGroups.Where(@group => @group.Id == department.ParentId).DefaultIfEmpty()
                from supervisorGroup in _orgGroupRepository.OrgGroups.Where(@group => @group.Id == devision.ParentId).DefaultIfEmpty()
                from depatmentSuper in _personRepository.People.Where(person => person.Id == department.Supervisor).DefaultIfEmpty()
                from devisionSuper in _personRepository.People.Where(person => person.Id == devision.Supervisor).DefaultIfEmpty()
                from supervisor in _personRepository.People.Where(person => person.Id == supervisorGroup.Supervisor).DefaultIfEmpty()
                select new {supervisor, supervisor.FirstName, personOnLeave, department, devision, supervisorGroup}).FirstOrDefault();
//            Console.WriteLine("Department super: {0}", result.dep?.FirstName);
            var super = _personRepository.People.FirstOrDefault(person => person.Id == result.supervisorGroup.Supervisor);
            Console.WriteLine("Person on leave: {0}", result.personOnLeave?.FirstName);
            Console.WriteLine("dep name: {0}", result.department?.GroupName);
            Console.WriteLine("dev name name: {0}", result.devision?.GroupName ?? "null");
            Console.WriteLine("Super group name name: {0}", result.supervisorGroup?.GroupName);
            Console.WriteLine("Super object first name: {0}", result.supervisor?.FirstName);
            Console.WriteLine("Super first name: {0}", result.FirstName);
            Console.WriteLine("second query, super: {0}", super?.FirstName);
            return result.supervisor;
        }

        public LeaveRequestService(OrgGroupRepository orgGroupRepository, PersonRepository personRepository
//            ,EmailService emailService
            )
        {
            _orgGroupRepository = orgGroupRepository;
            _personRepository = personRepository;
//            _emailService = emailService;
        }
    }
}