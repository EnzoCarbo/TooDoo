using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TooDoo.Models;

public readonly record struct YearMonth : IComparable<YearMonth>
{
    public YearMonth(int year, int month)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");
        }

        Year = year;
        Month = month;
    }
   
    public int Year { get; }
    public int Month { get; }

    public YearMonth Next()
    {
        if (Month == 12)
        {
            return new YearMonth(Year + 1, 1);
        }
        else
        {
            return new YearMonth(Year, Month + 1);
        }
    }

    public YearMonth Previous()
    {
        if (Month == 1)
        {
            return new YearMonth(Year - 1, 12);
        }
        else
        {
            return new YearMonth(Year, Month - 1);
        }

    }

    public static YearMonth From(DateTime date)
    {
       return new YearMonth(date.Year, date.Month);
    }

    public int CompareTo(YearMonth other)
    {
        int yearComparaison = Year.CompareTo(other.Year);

        if (yearComparaison != 0)
        {
            return yearComparaison;
        }
        
        return Month.CompareTo(other.Month);

    }

    public static bool operator <(YearMonth left, YearMonth right) => left.CompareTo(right) < 0;
    public static bool operator >(YearMonth left, YearMonth right) => left.CompareTo(right) > 0;
    public static bool operator >=(YearMonth left, YearMonth right) => left.CompareTo(right) >= 0;
    public static bool operator <=(YearMonth left, YearMonth right) => left.CompareTo(right) <= 0;


    public static YearMonth Parse(string s)
    {
        ArgumentNullException.ThrowIfNull(s);

        if (s.Length != 7 || s[4] != '-')
        {
            throw new FormatException($"Invalid year-month: '{s}'. Expected format: yyyy-MM.");
        }

        if (!int.TryParse(s.Substring(0, 4), out int year)
            || !int.TryParse(s.Substring(5, 2), out int month)
            || month < 0 || month > 12)
        {
            throw new FormatException($"Invalid month: '{s}'. Expected Month between 1 and 12");
        }

        return new YearMonth(year, month);
    }


    public override string ToString()
    {
            var year = Year.ToString("D4");
            var month = Month.ToString("D2");

            return year + "-" + month;
    }


}