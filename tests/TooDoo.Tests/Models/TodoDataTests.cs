using System.Windows.Controls;
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

            Assert.Empty(data.Tasks);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void AddTask_WithBlankTitle_Throws(string title)
        {
            var data = new TodoData();
            var now = new DateTime(2026, 10,31, 15, 8, 30, 0);

            Assert.Throws<ArgumentException>(() => data.AddTask(title, null, now));

        }
    }



}
