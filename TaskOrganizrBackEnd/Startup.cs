using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(TaskOrganizrBackEnd.Startup))]

namespace TaskOrganizrBackEnd
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMobileApp(app);
        }
    }
}