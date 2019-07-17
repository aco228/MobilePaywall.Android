using Microsoft.AspNet.SignalR;
using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Hubs
{
  public class LiveDeviceHub : Hub
  {

    public static LiveDeviceHub Current
    {
      get
      {
        return new LiveDeviceHub(GlobalHost.ConnectionManager.GetHubContext<LiveDeviceHub>());
      }
    }

    private IHubContext _context = null;

    public LiveDeviceHub(IHubContext context)
    {
      this._context = context;
    }

    public void Update(string sessionID, string tag, string text, bool fromDevice, DateTime created)
    {
    
      var data = new { 
        sessionID = sessionID,
        tag = tag,
        text = text,
        fromDevice = fromDevice,
        created = created.ToString()
      };

      if(this._context != null)
        this._context.Clients.All.update(data);
      else
        this.Clients.All.update(data);

    }

  }
}