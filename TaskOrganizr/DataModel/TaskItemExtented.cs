using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDLApi
{
    class TaskItemExtented:TaskItem
    {
        public TaskItemExtented(TaskItem task)
        {
            Id = task.Id;
            TaskName = task.TaskName;
            Description = task.Description;
            DateCreated = task.DateCreated;
            Deadline = task.Deadline;
            Notified = task.Notified;
            Completed = task.Completed;
            Expired = task.Expired;
            Priority = task.Priority;
            UserId = task.UserId;
            ListId = task.ListId;

            foreach(List list in API.totalLists)
            {
                if (ListId.Equals(list.Id))
                {
                    ListName = list.ListName;
                    break;
                }
            }

            this.task = task;
        }

        public TaskItem task { get; set; }

        public string ListName { get; set; }
    }
}
