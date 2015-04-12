using Microsoft.Owin;
using Owin;
using KitaabgharDownloader;

namespace KitaabgharDownloader
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}