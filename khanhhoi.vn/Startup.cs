using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(khanhhoi.vn.Startup))]
namespace khanhhoi.vn
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
