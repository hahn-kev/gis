using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class HolidayController
{
    [HttpGet]
    public List<Holiday> Holidays() {
        return new List<Holiday> {
            new Holiday {Name = "Christmas", Start = new DateTime(2019, 12, 24), End = new DateTime(2019, 12, 30)},
            new Holiday {Name = "New Years Day", Start = new DateTime(2019, 1, 1), End = new DateTime(2019, 1, 1)},
            new Holiday {Name = "Songkran", Start = new DateTime(2019, 4, 16), End = new DateTime(2019, 4, 17)},
        };
    }
}