namespace TooDoo.Models
{
    public class SubTask
    {
        public long ID { get; set; }    
        public required string Title { get; set; }
        public bool IsCompleted { get; set; }

    }
}
