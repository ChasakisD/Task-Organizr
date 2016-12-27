using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDLApi
{
    public class UserSettings
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "themeId")]
        public int themeId { get; set; }

        [JsonProperty(PropertyName = "notifyTasksOneDayBefore")]
        public bool NotifyTasksOneDayBefore { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}
