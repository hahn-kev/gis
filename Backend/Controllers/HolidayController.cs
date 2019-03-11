using System;
using System.Collections.Generic;
using Backend.Controllers;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class HolidayController : MyController
{
    private readonly IEntityService _entityService;

    public HolidayController(IEntityService entityService)
    {
        _entityService = entityService;
    }



    [HttpGet]
    public List<Holiday> Holidays()
    {
        return _entityService.List<Holiday>();
    }

    [HttpGet("current")]
    public List<Holiday> CurrentHolidays()
    {
        //todo filter by date
        return _entityService.List<Holiday>();
    }



    [HttpGet("{id}")]
    [Authorize(Policy = "jobs")]
    public Holiday GetById(Guid id)
    {
        return _entityService.GetById<Holiday>(id);
    }

    [HttpPost]
    [Authorize("holidays")]
    public Holiday Save([FromBody] Holiday holiday)
    {
        _entityService.Save(holiday);
        return holiday;
    }

    [HttpDelete]
    public IActionResult Delete(Guid id)
    {
        _entityService.Delete<Holiday>(id);
        return Ok();
    }
}