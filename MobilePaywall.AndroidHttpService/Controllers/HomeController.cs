using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobilePaywall.AndroidHttpService.Controllers
{
  public class HomeController : Controller
  {

    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (HomeController._log == null)
          HomeController._log = LogManager.GetLogger(typeof(HomeController));
        return HomeController._log;
      }
    }
    #endregion

    public ActionResult Index()
    {
      return this.Content("<strong>Work in progress..</strong>");
    }
  }
}