using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobilePaywall.AndroidHttpService.Controllers
{
  public class PortalController : Controller
  {

    public ActionResult Index()
    {
      return View("~/Views/Portal/Index.cshtml");
    }

    public ActionResult Devices()
    {
      return View("~/Views/Portal/Devices.cshtml");
    }
    public ActionResult Live()
    {
      return View("~/Views/Portal/Live.cshtml");
    }



  }
}