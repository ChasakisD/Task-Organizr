using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using TaskOrganizrBackEnd.DataObjects;
using TaskOrganizrBackEnd.Models;

namespace TaskOrganizrBackEnd.Controllers
{
    public class ListController : TableController<List>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            TaskOrganizrBackEndContext context = new TaskOrganizrBackEndContext();
            DomainManager = new EntityDomainManager<List>(context, Request);
        }

        // GET tables/List
        public IQueryable<List> GetAllList()
        {
            return Query(); 
        }

        // GET tables/List/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<List> GetList(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/List/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<List> PatchList(string id, Delta<List> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/List
        public async Task<IHttpActionResult> PostList(List item)
        {
            List current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/List/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteList(string id)
        {
             return DeleteAsync(id);
        }
    }
}
