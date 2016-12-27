using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDLApi
{
    public class Category
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "catName")]
        public string CatName { get; set; }

        [JsonProperty(PropertyName = "hex")]
        public string Hex { get; set; }

        public string UserId { get; set; }
    }
}
