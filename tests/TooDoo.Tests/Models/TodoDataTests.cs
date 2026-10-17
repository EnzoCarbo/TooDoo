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
    }





}
