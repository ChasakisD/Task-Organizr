using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskOrganizrBackEnd.DataObjects
{
    public class User:EntityData
    {
        public string UserProviderId { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Provider { get; set; }

        public string Password { get; set; }

        public string AccessToken { get; set; }

        public string Email { get; set; }

        public string Gender { get; set; }

        public string ImageUri { get; set; }

        public bool FirstJoin { get; set; }
    }
}