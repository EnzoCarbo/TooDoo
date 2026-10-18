using System.Drawing;
using System.Xml.Linq;
using TooDoo.Models;

namespace TooDoo.Tests.Models
{

    public class TodoDataTests
    {
        [Fact]
        public void AddTask_TrimsTitle_AndSetsDates()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 15, 9, 30, 0);
            var month = new YearMonth(2026, 10);

            var task = data.AddTask("  Payer loyer  ", month, now);


            Assert.Equal("Payer loyer", task.Title);

            Assert.Equal(now, task.CreatedAt);

            Assert.Equal(now, task.UpdatedAt);

            Assert.Equal(month, task.Month);

            Assert.NotEqual(Guid.Empty, task.Id);

            Assert.Contains(task, data.Tasks);

        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void AddTask_WithBlankTitle_Throws(string title)
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 31, 15, 8, 30, 0);

            Assert.Throws<ArgumentException>(() => data.AddTask(title, null, now));

            Assert.Empty(data.Tasks);

        }

        [Fact]

        public void TasksIn_Null_ReturnsOnlyBacklog()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 31, 10, 10, 10, 30);
            var month = new YearMonth(2026, 10);
            var backlogTask = data.AddTask("Titre test 1", null, now);
            data.AddTask("Titre test 2", month, now);
            var result = data.TasksIn(null);

            var only = Assert.Single(result);
            Assert.Equal(backlogTask, only);


        }

        [Fact]
        public void TasksIn_Month_ReturnsOnlyThatMonth_InInsertionOrder()
        {


            var data = new TodoData();
            var now = new DateTime(2026, 10, 31, 10, 10, 10, 30);
            var october = new YearMonth(2026, 10);
            var november = new YearMonth(2026, 11);

            var first = data.AddTask("Titre test ", october, now);
            var second = data.AddTask("Titre test 2", november, now);
            var third = data.AddTask("Titre test 3", october, now);

            var result = data.TasksIn(october);

            Assert.Equal(new[] { first, third }, result);
        }

        [Fact]
        public void RenameTask_ChangesTitle_AndUpdatesUpdatedAt()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task = data.AddTask("Task 1", october, now);

            var renamed = data.RenameTask(task.Id, "  Nouveau titre  ", later);

            Assert.True(renamed);

            Assert.Equal("Nouveau titre", task.Title);
            Assert.Equal(later, task.UpdatedAt);
            Assert.Equal(now, task.CreatedAt);

        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]

        public void RenameTask_WithBlankTitle_Throws(string title)
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task = data.AddTask("Task 1", october, now);

            Assert.Throws<ArgumentException>(() => data.RenameTask(task.Id, title, later));
            Assert.Equal("Task 1" ,task.Title);
            Assert.Equal(now, task.UpdatedAt);
        }


        [Fact]
        public void RenameTask_UnknownId_ReturnsFalse()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task = data.AddTask("Task 1", october, now);

            var renamed = data.RenameTask(Guid.NewGuid(), "Autre titre", later);
            Assert.False(renamed);

            Assert.Equal("Task 1", task.Title);
            Assert.Equal(now, task.UpdatedAt);
            Assert.Equal(now, task.CreatedAt);
        }



        [Fact]
        public void SetTaskDone_UpdatesUpdatedAt()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task = data.AddTask("Task 1", october, now);

            var taskDone = data.SetTaskDone(task.Id, true, later);

            Assert.True(taskDone);

            Assert.True(task.IsDone);
            Assert.Equal(later, task.UpdatedAt);
        }

        [Fact]
        public void MoveTask_ToBacklog_SetsMonthNull()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task = data.AddTask("Task 1", october, now);

            var moved = data.MoveTask(task.Id, null, later);

            Assert.True(moved);

            Assert.Null(task.Month);

            Assert.Equal(later, task.UpdatedAt);
        }
        [Fact]
        public void DeleteTask_RemovesIt()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, now);

            var deleted = data.DeleteTask(task1.Id);

            Assert.True(deleted);
            Assert.Single(data.Tasks, task2);
            Assert.DoesNotContain(task1, data.Tasks);

        }

        [Fact]
        public void DeleteTask_UnknownId_ReturnsFalse()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, now);

            var deleted = data.DeleteTask(Guid.NewGuid());

            Assert.False(deleted);
            Assert.Contains(task1, data.Tasks);
            Assert.Contains(task2, data.Tasks);

        }

        [Fact]
        public void AddSubTask_UpdatesParentUpdatedAt()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, later);

            var subTask = data.AddSubTask(task1.Id, "  Sub Task 1  ", later);

            Assert.NotNull(subTask);
            Assert.NotEqual(task1.Id, subTask.Id);
            Assert.Equal(later, task1.UpdatedAt);
            Assert.Contains(subTask, task1.SubTasks);
            Assert.False(subTask.IsDone);
            Assert.Equal("Sub Task 1", subTask.Title);
            Assert.Empty(task2.SubTasks);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void AddSubTask_WithBlankTitle_Throws(string title)
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 31, 15, 8, 30, 0);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);

            Assert.Throws<ArgumentException>(() => data.AddSubTask(task1.Id, title, now));

            Assert.Empty(task1.SubTasks);

        }

        [Fact]
        public void AddSubTask_UnknownTask_ReturnsNull()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, later);

            var subTask = data.AddSubTask(Guid.NewGuid(), "  Sub Task 1  ", now);

            Assert.Null(subTask);
            Assert.Empty(task1.SubTasks);
            Assert.Empty(task2.SubTasks);

        }

        [Fact]
        public void RenameSubTask_ChangesTitle_AndUpdatesParent()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, later);

            var subTask1 = data.AddSubTask(task1.Id, "  Sub Task 1  ", now);
            var subTask2 = data.AddSubTask(task1.Id, "  Sub Task 2  ", now);
            Assert.NotNull(subTask1);
            Assert.NotNull(subTask2);

            var renamed = data.RenameSubTask(task1.Id, subTask1.Id, "  Rename Sub Task 1  ", later);

            Assert.True(renamed);
            Assert.Equal("Rename Sub Task 1", subTask1.Title);
            Assert.Equal(later, task1.UpdatedAt);
            Assert.Equal("Sub Task 2", subTask2.Title);

        }

        [Fact]
        public void SetSubTaskDone_ChecksSubTask_AndUpdatesParent()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, later);

            var subTask1 = data.AddSubTask(task1.Id, "  Sub Task 1  ", now);
            var subTask2 = data.AddSubTask(task1.Id, "  Sub Task 2  ", now);

            Assert.NotNull(subTask1);
            Assert.NotNull(subTask2);

            var completed = data.SetSubTaskDone(task1.Id, subTask1.Id, true, later); 

            Assert.True(completed);
            Assert.True(subTask1.IsDone);
            Assert.False(subTask2.IsDone);
            Assert.Equal(later, task1.UpdatedAt);
        }

        [Fact]
        public void SetSubTaskDone_UnknownSubTask_ReturnsFalse()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, later);

            var subTask1 = data.AddSubTask(task1.Id, "  Sub Task 1  ", now);
            var subTask2 = data.AddSubTask(task1.Id, "  Sub Task 2  ", now);

            Assert.NotNull(subTask1);
            Assert.NotNull(subTask2);

            var completed = data.SetSubTaskDone(task1.Id, Guid.NewGuid(), true, later);

            Assert.False(completed);
            Assert.False(subTask1.IsDone);
            Assert.False(subTask2.IsDone);
            Assert.Equal(now, task1.UpdatedAt);
        }

        [Fact]
        public void DeleteSubTask_RemovesIt_AndUpdatesParent()
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, later);

            var subTask1 = data.AddSubTask(task1.Id, "  Sub Task 1  ", now);
            var subTask2 = data.AddSubTask(task1.Id, "  Sub Task 2  ", now);

            Assert.NotNull(subTask1);
            Assert.NotNull(subTask2);

            var deleted = data.DeleteSubTask(task1.Id, subTask1.Id, later);

            Assert.True(deleted);
            Assert.DoesNotContain(subTask1, task1.SubTasks);
            Assert.Equal(later, task1.UpdatedAt);
            Assert.Contains(subTask2, task1.SubTasks);
        }
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void RenameSubTask_WithBlankTitle_Throws(string title)
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10, 10, 10, 10, 10, 10, 10);
            var later = now.AddHours(1);
            var october = new YearMonth(2026, 10);
            var task1 = data.AddTask("Task 1", october, now);
            var task2 = data.AddTask("Task 2 ", october, later);

            var subTask1 = data.AddSubTask(task1.Id, "  Sub Task 1  ", now);
            var subTask2 = data.AddSubTask(task1.Id, "  Sub Task 2  ", now);
            Assert.NotNull(subTask1);
            Assert.Throws<ArgumentException>(() => data.RenameSubTask(task1.Id, subTask1.Id, title, later));
            Assert.Equal("Sub Task 1", subTask1.Title);
            Assert.Equal(now, task1.UpdatedAt);
        }
    }
}
