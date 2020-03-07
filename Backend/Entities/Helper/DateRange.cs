using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Entities.Helper
{
    public struct DateRange
    {
        public DateRange((DateTime, DateTime) tuple) : this(tuple.Item1, tuple.Item2)
        {
        }

        public DateRange(DateTime start, DateTime end)
        {
            if (end < start) throw new ArgumentException("end is less than start date");
            Start = start;
            End = end;
        }

        public DateTime Start { get; }
        public DateTime End { get; }

        public bool Includes(DateTime value)
        {
            return (Start <= value) && (value <= End);
        }

        public bool Includes(DateRange range)
        {
            return (Start <= range.Start) && (range.End <= End);
        }

        public bool Overlaps(DateRange range)
        {
            if (Includes(range) || range.Includes(this)) return true;
            return Start <= range.End && range.Start <= End;
        }

        public (DateRange, DateRange?) Combine(DateRange range)
        {
            return Combine(this, range);
        }

        public static (DateRange, DateRange?) Combine(DateRange range1, DateRange range2)
        {
            if (range1.Overlaps(range2))
            {
                return (
                    new DateRange(Min(range1.Start, range2.Start), Max(range1.End, range2.End)),
                    null
                );
            }

            return (range1, range2);
        }

        public static IEnumerable<DateRange> Combine(IEnumerable<DateRange> ranges)
        {
            DateRange? currentRange = null;
            foreach (var range in ranges.OrderBy(r => r.Start))
            {
                if (currentRange == null)
                {
                    currentRange = range;
                    continue;
                }

                DateRange? newRange;
                (currentRange, newRange) = Combine(currentRange.Value, range);
                if (!newRange.HasValue) continue;

                yield return currentRange.Value;
                currentRange = newRange;
            }

            if (currentRange.HasValue)
                yield return currentRange.Value;
        }

        public static DateTime Min(DateTime d1, DateTime d2)
        {
            return d1 < d2 ? d1 : d2;
        }

        public static DateTime Max(DateTime d1, DateTime d2)
        {
            return d1 < d2 ? d2 : d1;
        }

        public TimeSpan Length => End - Start;

        public static implicit operator DateRange((DateTime, DateTime) t) => new DateRange(t);

        public bool Equals(DateRange other)
        {
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj)
        {
            return obj is DateRange other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        public static bool operator ==(DateRange left, DateRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DateRange left, DateRange right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{Start} .. {End}";
        }
    }
}