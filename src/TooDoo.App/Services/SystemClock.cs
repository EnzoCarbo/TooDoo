using TooDoo.Abstractions;

namespace TooDoo.Services;

public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}
