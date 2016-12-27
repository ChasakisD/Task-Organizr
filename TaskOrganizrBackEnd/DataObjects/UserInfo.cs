using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskOrganizrBackEnd.DataObjects
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Gender { get; set; }
        public string ImageUri { get; set; }
    }
}