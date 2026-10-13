using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TooDoo.Models;

namespace TooDoo.Tests.Models;

public class YearMonthTests
{
    [Fact]
    public void Next_OfDecember_IsJanuaryOfNextYear()
    {
        var december = new YearMonth(2026, 12);

        var next = december.Next();

        Assert.Equal(new YearMonth(2027, 1), next);
    }


    [Fact]

    public void Next_OfApril_IsMay_OfSameYear()
    {
        var april = new YearMonth(2026, 4);

        var next = april.Next();

        Assert.Equal(new YearMonth(2026,5), next);
    }

    }