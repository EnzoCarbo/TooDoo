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
    public void Next_OfApril_IsMayOfSameYear()
    {
        var april = new YearMonth(2026, 4);

        var next = april.Next();

        Assert.Equal(new YearMonth(2026,5), next);
    }

    [Fact]
    public void Previous_OfJanuary_IsDecemberOfPreviousYear()
    {
        var january = new YearMonth(2026, 1);

        var previous = january.Previous();

        Assert.Equal(new YearMonth(2025,12), previous);

    }
    [Fact]
    public void Previous_OfApril_IsMarchOfSameYear()
    {
        var april = new YearMonth(2026, 4);

        var previous = april.Previous();

        Assert.Equal(new YearMonth(2026, 3), previous);
    }

}