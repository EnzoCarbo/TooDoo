using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TooDoo.Models
{
    public class TodoTask
    {
        public Guid Id { get; init; }
        public required string Title { get; set; }
        public bool IsDone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public YearMonth? Month { get; set; }
        public List<SubTask> SubTasks { get; set; } = new(); 
    }
}   
