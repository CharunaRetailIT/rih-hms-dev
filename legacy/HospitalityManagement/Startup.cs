using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HospitalityManagement.Startup))]
namespace HospitalityManagement
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
