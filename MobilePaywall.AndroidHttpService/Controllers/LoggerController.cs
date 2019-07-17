using log4net;
using MobilePaywall.AndroidHttpService.Code.Tasks;
using MobilePaywall.AndroidHttpService.Hubs;
using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobilePaywall.AndroidHttpService.Controllers
{
  public class LoggerController : Controller
  {

    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (LoggerController._log == null)
          LoggerController._log = LogManager.GetLogger(typeof(LoggerController));
        return LoggerController._log;
      }
    }
    #endregion

    /// SUMMARY: Collects logs from applications
    public ActionResult Index()
    {
      string sessionID = Request["sessionID"] != null ? Request["sessionID"] : string.Empty;
      string tag = Request["tag"] != null ? Request["tag"] : string.Empty;
      string text = Request["text"] != null ? Request["text"] : string.Empty;

      AndroidClientLog.Log(sessionID, tag, text);

      // ping text, with that we will trigger adding new task
      if (text.Equals("."))
        System.Threading.Tasks.Task.Run(() => PaywallApplication.TaskManager.AddTask(new PayTask(sessionID)));

      LiveDeviceHub.Current.Update(sessionID, tag, text, true, DateTime.Now);
      return this.Json(new { s = 1}, JsonRequestBehavior.AllowGet);
    }
    
  }



}