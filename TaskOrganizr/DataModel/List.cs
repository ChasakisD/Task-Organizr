using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDLApi
{
    public class List
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "listName")]
        public string ListName { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "categoryId")]
        public string CategoryId { get; set; }

        [JsonProperty(PropertyName = "sortPreference")]
        public int SortPreference { get; set; }
    }
}
