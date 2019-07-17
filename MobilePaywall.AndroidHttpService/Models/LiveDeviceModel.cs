using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Models
{
  public class LiveDeviceModel
  {
    private AndroidClientSession _session = null;
    private List<AndroidClientLog> _logs = null;

    public AndroidClientSession Session { get { return this._session; } }
    public List<AndroidClientLog> Logs { get { return this._logs; } }

    public LiveDeviceModel(AndroidClientSession session, List<AndroidClientLog> logs)
    {
      this._session = session;  
      this._logs = logs;
    }
  }
}