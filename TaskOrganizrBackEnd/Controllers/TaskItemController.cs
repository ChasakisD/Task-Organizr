using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using TaskOrganizrBackEnd.DataObjects;
using TaskOrganizrBackEnd.Models;
using System.Collections.Generic;
using Microsoft.Azure.Mobile.Server.Config;
using System;
using System.Web;
using Microsoft.Azure.NotificationHubs;

namespace TaskOrganizrBackEnd.Controllers
{
    public class TaskItemController : TableController<TaskItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            TaskOrganizrBackEndContext context = new TaskOrganizrBackEndContext();
            DomainManager = new EntityDomainManager<TaskItem>(context, Request);
        }

        // GET tables/TaskItem
        public IQueryable<TaskItem> GetAllTaskItem()
        {
            return Query(); 
        }

        // GET tables/TaskItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TaskItem> GetTaskItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TaskItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<TaskItem> PatchTaskItem(string id, Delta<TaskItem> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/TaskItem
        public async Task<IHttpActionResult> PostTaskItem(TaskItem item)
        {
            TaskItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TaskItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTaskItem(string id)
        {
             return DeleteAsync(id);
        }
    }
}
