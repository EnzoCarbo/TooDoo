using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TooDoo.Models;

public readonly record struct YearMonth
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

} 