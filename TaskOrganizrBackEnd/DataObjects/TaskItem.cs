using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskOrganizrBackEnd.DataObjects
{
    public class TaskItem:EntityData
    {
        public string TaskName { get; set; }

        public string Description { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime Deadline { get; set; }

        public bool Notified { get; set; }

        public bool Completed { get; set; }

        public bool Expired { get; set; }

        public int Priority { get; set; }

        public string UserId { get; set; }

        public string ListId { get; set; }
    }
}