using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskOrganizrBackgroundTask
{
    public sealed class TaskItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "taskName")]
        public string TaskName { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTimeOffset DateCreated { get; set; }

        [JsonProperty(PropertyName = "deadline")]
        public DateTimeOffset Deadline { get; set; }

        [JsonProperty(PropertyName = "notified")]
        public bool Notified { get; set; }

        [JsonProperty(PropertyName = "completed")]
        public bool Completed { get; set; }

        [JsonProperty(PropertyName = "expired")]
        public bool Expired { get; set; }

        [JsonProperty(PropertyName = "priority")]
        public int Priority { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "listId")]
        public string ListId { get; set; }
    }
}
