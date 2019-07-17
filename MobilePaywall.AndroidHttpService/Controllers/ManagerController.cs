using MobilePaywall.AndroidHttpService.Models;
using MobilePaywall.Data;
using MobilePaywall.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobilePaywall.AndroidHttpService.Controllers
{
  public class ManagerController : Controller
  {
    // GET: Manager
    public ActionResult Index()
    {
      List<AndroidPremiumCustomer> android = AndroidPremiumCustomer.CreateManager().Load();
      ManagerModel model = new ManagerModel();
      
      model.Customers = android;

      return View("Index", model);
    }
    public ActionResult FilterTable()
    {
      string _top = Request["top"] != null ? Request["top"].ToString() : string.Empty;
      string _from = Request["from"] != null ? Request["from"].ToString() : string.Empty;
      string _to = Request["to"] != null? Request["to"].ToString() : string.Empty;
      string _country = Request["country"] != null? Request["country"].ToString() : string.Empty;

      int countryID;
      if (!Int32.TryParse(_country, out countryID))
        return Content("Country Id is not set");

      int top;
      if (!Int32.TryParse(_top, out top))
        return Content("Top is not set");

      DateTime from = new DateTime();
      if (!DateTime.TryParse(_from , out from))
        return Content("Cannot parse from date!");
      DateTime to = new DateTime();
      if (!DateTime.TryParse(_to , out to))
        return Content("Cannot parse to date!");
     
      Country country = new Country(countryID);

      List<AndroidPremiumCustomer> customer = AndroidPremiumCustomer.CreateManager().Load(from, to, country, top);

      ManagerModel model = new ManagerModel();
      model.Customers = customer;

      if (model == null)
        return Content("Model is empty");

      return PartialView("~/Views/Manager/Partials/_Customers.cshtml", model);
    }
  }
}