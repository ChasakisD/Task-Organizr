using Microsoft.Azure.Mobile.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskOrganizrBackEnd.DataObjects
{
    public class Category:EntityData
    {
        public string CatName { get; set; }

        public string Hex { get; set; }

        public string UserId { get; set; }
    }
}