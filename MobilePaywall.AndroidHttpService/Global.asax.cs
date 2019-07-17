using log4net;
using MobilePaywall.AndroidHttpService.Code.TaskBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MobilePaywall.AndroidHttpService
{
  public class PaywallApplication : System.Web.HttpApplication
  {

    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (PaywallApplication._log == null)
          PaywallApplication._log = LogManager.GetLogger(typeof(PaywallApplication));
        return PaywallApplication._log;
      }
    }
    #endregion

    public static TaskManager TaskManager = null;

    protected void Application_Start()
    {
      MobilePaywall.Data.Sql.Database dummy = null;
      Senti.Data.DataLayerRuntime r = new Senti.Data.DataLayerRuntime();

      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(Server.MapPath("~/log4net.config")));
      
      PaywallApplication.TaskManager = new TaskManager();

      new Thread(() => { 
        PaywallApplication.TaskManager.Run();
      }).Start();

      MobilePaywall.AndroidHttpService.Code.Session.GetSuitableServices.GetWapService(MobilePaywall.Data.AndroidClientSession.CreateManager().Load(33));

      Log.Debug("Application started");

    }

    protected void Application_Error(object sender, EventArgs e)
    {
      Exception exception = Server.GetLastError();
      Server.ClearError();

      Log.Fatal("FATAL", exception);
    }


  }
}
