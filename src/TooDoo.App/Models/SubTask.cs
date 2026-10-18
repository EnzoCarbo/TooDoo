namespace TooDoo.Models;

public class SubTask
{
    public Guid Id { get; init; }    
    public required string Title { get; set; }
    public bool IsDone { get; set; }

}
