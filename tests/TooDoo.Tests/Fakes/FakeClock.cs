using TooDoo.Abstractions;

namespace TooDoo.Tests.Fakes;

public class FakeClock : IClock
{
    public DateTime Now { get; set; } = new DateTime(2026, 10, 15, 9, 30, 0);
}