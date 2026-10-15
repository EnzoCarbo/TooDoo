using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TooDoo.Models
{
    public class TodoData
    {

        public int SchemaVersion { get; set; } = 1;

        public List<TodoTask> Tasks { get; set; } = new();

        public TodoTask AddTask(string title, YearMonth? month, DateTime now)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            var task = new TodoTask
            {
                Id = Guid.NewGuid(),
                Title = title.Trim(),
                Month = month,
                CreatedAt = now,
                UpdatedAt = now,
            };


            Tasks.Add(task);

            return task;


        }
    }
}





