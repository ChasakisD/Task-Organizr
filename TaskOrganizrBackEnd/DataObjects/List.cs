using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskOrganizrBackEnd.DataObjects
{
    public class List:EntityData
    {
        public string ListName { get; set; }

        public string UserId { get; set; }

        public int SortPreference { get; set; }

        public string CategoryId { get; set; }
    }
}