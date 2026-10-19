using TooDoo.Abstractions;
using TooDoo.Models;

namespace TooDoo.Services;

public class InMemoryTodoStore : ITodoStore
{
    public TodoData Data { get; } = new();

    public event EventHandler? DataChanged;

    public void Update(Action<TodoData> mutation)
    {
        mutation(Data);
        DataChanged?.Invoke(this, EventArgs.Empty);
        return;

    }
}
