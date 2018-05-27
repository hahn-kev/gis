using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using Backend.Services;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class TrainingController : Controller
    {
        private readonly TrainingService _trainingService;

        public TrainingController(TrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpGet]
        [Authorize(Policy = "training")]
        public IList<TrainingRequirement> List()
        {
            return _trainingService.TrainingRequirements;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "training")]
        public TrainingRequirement Get(Guid id)
        {
            return _trainingService.GetById(id);
        }

        [HttpPost]
        [Authorize(Policy = "training")]
        public TrainingRequirement Save([FromBody] TrainingRequirement requirement)
        {
            _trainingService.Save(requirement);
            return requirement;
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "training")]
        public void DeleteRequirement(Guid id)
        {
            _trainingService.DeleteRequirement(id);
        }

        [HttpGet("staff/year/{year}")]
        [Authorize(Policy = "training")]
        public IList<StaffTraining> StaffTrainings(int year)
        {
            return _trainingService.GetByYear(year);
        }

        [HttpGet("staff/{staffId}")]
        public IList<StaffTrainingWithRequirement> StaffTrainingWithRequirements(Guid staffId)
        {
            return _trainingService.GetByStaff(staffId);
        }

        [HttpPost("staff")]
        [Authorize(Policy = "training")]
        public StaffTraining Save([FromBody] StaffTraining staffTraining)
        {
            _trainingService.Save(staffTraining);
            return staffTraining;
        }

        [HttpDelete("staff/{id}")]
        [Authorize(Policy = "training")]
        public void DeleteStaffTraining(Guid id)
        {
            _trainingService.DeleteStaffTraining(id);
        }

        [HttpPost("staff/allComplete")]
        [Authorize(Policy = "training")]
        public IActionResult MarkAllComplete([FromBody] List<Guid> staffIds, Guid? requirementId,
            DateTime? completeDate)
        {
            if (completeDate == null) throw new ArgumentNullException(nameof(completeDate));
            if (!requirementId.HasValue || requirementId.Value == Guid.Empty)
                throw new ArgumentNullException(nameof(requirementId));
            _trainingService.MarkAllComplete(staffIds, requirementId.Value, completeDate.Value);
            return Ok();
        }
    }
}