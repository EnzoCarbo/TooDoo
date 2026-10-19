using TooDoo.Models;
using TooDoo.Services;

namespace TooDoo.Tests.Services;

public class InMemoryTodoStoreTests
{
    [Fact]
    public void Update_AppliesMutation()
    {
        var store = new InMemoryTodoStore();
        store.Update(d => d.AddTask("A", null, new DateTime(2026, 10, 15)));
        Assert.Single(store.Data.Tasks);
        Assert.Equal("A", Assert.Single(store.Data.Tasks).Title);
    }


    [Fact]
    public void Update_RaisesDataChanged()
    {
        var store = new InMemoryTodoStore();
        var raised = 0;
        store.DataChanged += (sender, e) => raised++;

        store.Update(d => d.AddTask("A", null, new DateTime(2026, 10, 15)));

        Assert.Equal(1, raised);
    }


}


