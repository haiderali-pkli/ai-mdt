using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BDCS.Startup))]
namespace BDCS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
