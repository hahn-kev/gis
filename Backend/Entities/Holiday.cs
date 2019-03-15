using System;
using Backend.Entities;

public class Holiday : BaseEntity
{
    public string Name { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public override string ToString()
    {
        return $"{Name} {Start:d} -> {End:d}";
    }
}