using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class TrainingController : Controller
    {
        private TrainingService _trainingService;

        public TrainingController(TrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpGet]
        public IList<TrainingRequirement> List()
        {
            return _trainingService.TrainingRequirements;
        }

        [HttpGet("{id}")]
        public TrainingRequirement Get(Guid id)
        {
            return _trainingService.GetById(id);
        }

        [HttpPost]
        public TrainingRequirement Save([FromBody] TrainingRequirement requirement)
        {
            _trainingService.Save(requirement);
            return requirement;
        }

        [HttpDelete("{id}")]
        public void Delete(Guid id)
        {
            _trainingService.Delete(id);
        }
    }
}