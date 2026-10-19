using TooDoo.Models;

namespace TooDoo.Abstractions;

public interface ITodoStore
{
    TodoData Data { get; }
    void Update(Action<TodoData> mutation);
    event EventHandler? DataChanged;
}
