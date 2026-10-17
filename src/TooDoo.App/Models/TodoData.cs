using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public IEnumerable<TodoTask> TasksIn(YearMonth? yearMonth)
        {
            return Tasks.Where(t => t.Month == yearMonth);
        }

        public bool RenameTask(Guid id, string title, DateTime now)
        {
            var task = FindTask(id);
            if (task == null)
            {
                return false;
            }

            task.Title = title.Trim();
            task.UpdatedAt = now;
            return true;
        }

        public bool SetTaskDone(Guid id, bool isDone, DateTime now)
        {
            var task = FindTask(id);
            if (task == null)
            {
                return false;
            }
            task.IsDone = isDone;
            task.UpdatedAt = now;
            return true;
        }

        public bool MoveTask(Guid id, YearMonth? yearMonth, DateTime now)
        {
            var task = FindTask(id);
            if (task == null)
            {
                return false;
            }
            task.Month = yearMonth;
            task.UpdatedAt = now;
            return true;
        }

        private TodoTask? FindTask(Guid id) => Tasks.FirstOrDefault(t => t.Id == id);

    }
}





