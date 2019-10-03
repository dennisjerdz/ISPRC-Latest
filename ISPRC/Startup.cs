using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ISPRC.Startup))]
namespace ISPRC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
