using Microsoft.Owin;
using Tanka.FileSystem.WebApiSample;

[assembly: OwinStartup(typeof(Startup))]

namespace Tanka.FileSystem.WebApiSample
{
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            StartupAuth.ConfigureAuth(app);
        }
    }
}
