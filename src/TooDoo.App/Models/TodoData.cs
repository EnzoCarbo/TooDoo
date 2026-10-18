using System.Diagnostics.CodeAnalysis;

namespace TooDoo.Models
{
    public class TodoData
    {

        public int SchemaVersion { get; set; } = 1;
        public List<TodoTask> Tasks { get; set; } = new();
        private TodoTask? FindTask(Guid id) => Tasks.FirstOrDefault(t => t.Id == id);

        private bool TryFindSubTask(Guid taskId, Guid subtaskId, [NotNullWhen(true)] out TodoTask? task, [NotNullWhen(true)] out SubTask? subTask)
        {
            task = FindTask(taskId);
            if (task == null)
            {
                subTask = null;
                return false;
            }
            subTask = task.SubTasks.FirstOrDefault(t => t.Id == subtaskId);
            return subTask != null;

        }


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

        public bool DeleteTask(Guid id)
        {
            var task = FindTask(id);
            if (task == null)
            {
                return false;
            }
            Tasks.Remove(task);
            return true;
        }

        public SubTask? AddSubTask(Guid taskId, string title, DateTime now)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            var task = FindTask(taskId);
            if (task == null)
            {
                return null;
            }
            var subtask = new SubTask
            {
                Id = Guid.NewGuid(),
                Title = title.Trim(),
            };
            task.UpdatedAt = now;
            task.SubTasks.Add(subtask);
            return subtask;
        }
        public bool RenameSubTask(Guid taskId, Guid subTaskId, string title, DateTime now)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            if (!TryFindSubTask(taskId, subTaskId, out var task, out var subtask))
            {
                return false; 
            }

            subtask.Title = title.Trim();
            task.UpdatedAt = now;

            return true;
        }
        public bool SetSubTaskDone(Guid taskId, Guid subTaskId, bool isDone, DateTime now)
        {
            if (!TryFindSubTask(taskId, subTaskId, out var task, out var subtask))
            { 
                return false;
            }
            subtask.IsDone = isDone;
            task.UpdatedAt = now;

            return true;

        }
        public bool DeleteSubTask(Guid taskId, Guid subTaskId, DateTime now)
        {
            if (!TryFindSubTask(taskId, subTaskId, out var task, out var subtask))
            {
                return false;
            }

            task.SubTasks.Remove(subtask);
            task.UpdatedAt = now;
            return true;
            }
    }
}





