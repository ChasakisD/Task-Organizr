using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskOrganizrBackgroundTask
{
    public sealed class User
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "userProviderId")]
        public string UserProviderId { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "imageUri")]
        public string ImageUri { get; set; }

        [JsonProperty(PropertyName = "firstJoin")]
        public bool FirstJoin { get; set; }
    }
}
