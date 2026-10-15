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
    }
}
