using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MobilePaywall.AndroidHttpService.SignalRStartup))]
namespace MobilePaywall.AndroidHttpService
{
  public class SignalRStartup
  {
    public void Configuration(IAppBuilder app)
    {
      app.MapSignalR();
    }
  }
}
