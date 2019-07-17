using log4net;
using MobilePaywall.Data;
using MobilePaywall.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobilePaywall.AndroidHttpService.Controllers
{
  public class PremiumController : Controller
  {

    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (PremiumController._log == null)
          PremiumController._log = LogManager.GetLogger(typeof(PremiumController));
        return PremiumController._log;
      }
    }
    #endregion

    // SUMMARY: Method for testing communication
    public ActionResult Index()
    {
      return this.Json(new { status = true, message = "OK" }, JsonRequestBehavior.AllowGet);
    }

    // GET: Entrance call from apk, first thing that will be called
    public ActionResult Call()
    {
      string uniqueID = Request["uniqueID"] != null ? Request["uniqueID"].ToString() : string.Empty;
      string msisdn = Request["msisdn"] != null ? Request["msisdn"].ToString() : string.Empty;
      string referrer = Request["referrer"] != null ? Request["referrer"].ToString() : string.Empty;
      string applicationName = Request["applicationName"] != null ? Request["applicationName"].ToString() : string.Empty;

      // Check values
      if (string.IsNullOrEmpty(uniqueID))
      {
        Log.Error("Primium.Entrance:: No uniqueID");
        return this.Json(new { status = false }, JsonRequestBehavior.AllowGet);
      }

      if (string.IsNullOrEmpty(applicationName))
      {
        Log.Error("Primium.Entrance:: No application name");
        return this.Json(new { status = false }, JsonRequestBehavior.AllowGet);
      }
      
      AndroidPremiumCustomer customer = this.GetCustomer(uniqueID, applicationName, Request.UserHostAddress, msisdn, referrer);

      // Find suitable service for subscription
      MobilePaywallDirect db = MobilePaywallDirect.Instance;
      PremiumSubscriptionModel subscriptionModel = PremiumSubscriptionModel.GetModel(db, customer);
      
      if (subscriptionModel == null)
      {
        Log.Debug("Primium.Entrance:: There is no suitable PSMS service for country=" + customer.Country.TwoLetterIsoCode);
        return this.Json(new { status = false, customerID=customer.ID}, JsonRequestBehavior.AllowGet);
      }
      
      string textmessage = string.Format("{0} /ac={1}", subscriptionModel.Keyword, customer.ID);

      AndroidPremiumCustomerServiceMap map = new AndroidPremiumCustomerServiceMap(-1,
        customer,
        null,
        Int32.Parse(subscriptionModel.ServiceID),
        subscriptionModel.Shortcode,
        textmessage,
        null,
        DateTime.Now, DateTime.Now);
      map.Insert();
      
      int minuteWait = (new Random()).Next(2, 7);
      
      Log.Debug(string.Format("Primium.Entrance:: Customer:{0}, Service:{1}, Shortcode:{2}, Keyword:{3}, Wait: {4}",
        customer.ID, subscriptionModel.ServiceName, subscriptionModel.Shortcode, subscriptionModel.Keyword, minuteWait));

      return this.Json(new
      {
        status = true,
        minute = minuteWait,
        customerID = customer.ID,
        shortcode = subscriptionModel.Shortcode,
        textmessage = textmessage,
      }, JsonRequestBehavior.AllowGet);
    }

    // GET Logger from application
    public ActionResult Logger()
    {
      string sessionID = Request["sessionID"] != null ? Request["sessionID"].ToString() : "";
      string tag = Request["tag"] != null ? Request["tag"].ToString() : "";
      string text = Request["text"] != null ? Request["text"].ToString() : "";

      if(string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(text))
      {
        Log.Error("Premium.Log:: There is no sessionID or tag or text");
        return this.Json(new { status = false }, JsonRequestBehavior.AllowGet);
      }

      int cID = 0;
      //if(!string.IsNullOrEmpty(sessionID))
      //{
      //  if (!Int32.TryParse(sessionID, out cID))
      //  {
      //    Log.Error("Premium.Log:: SessionID could not be parsed");
      //    return this.Json(new { status = false }, JsonRequestBehavior.AllowGet);
      //  }

      //  AndroidPremiumCustomer customer = AndroidPremiumCustomer.CreateManager().Load(cID);
      //  if (customer == null)
      //  {
      //    Log.Error("Premium.Log:: Threre is no customer with ID=" + cID);
      //    return this.Json(new { status = false }, JsonRequestBehavior.AllowGet);
      //  }
      //}
      
      Log.Debug(string.Format("Premium.Log:: c={0}.. {1}:{2}", cID, tag, text));
      return this.Json(new { status = true }, JsonRequestBehavior.AllowGet);
    }

    private AndroidPremiumCustomer GetCustomer(string uniqueID, string applicationName, string ipAddress, string msisdn, string referrer)
    {
      AndroidPremiumCustomer customer = AndroidPremiumCustomer.CreateManager().Load(uniqueID);
      if (customer != null)
        return customer;

      AndroidPremiumAplication app = AndroidPremiumAplication.CreateManager().Load(applicationName);
      if(app == null)
      {
        app = new AndroidPremiumAplication(-1, applicationName, DateTime.Now, DateTime.Now);
        app.Insert();
      }

      if (ipAddress == "::1")
        ipAddress = "62.4.59.218";

      IIPCountryMapManager ipcmManager = IPCountryMap.CreateManager();
      IPCountryMap countryMap = ipcmManager.Load(ipAddress);

      if (countryMap == null || countryMap.Country == null)
      {
        Log.Fatal("Could not load Country by ID:" + ipAddress);
        return null;
      }

      customer = new AndroidPremiumCustomer(-1,
        uniqueID,
        app,
        countryMap.Country,
        ipAddress,
        msisdn,
        referrer,
        DateTime.Now, DateTime.Now);
      customer.Insert();

      return customer; 
    }


  }

  public class PremiumSubscriptionModel
  {
    public string ServiceID = string.Empty;
    public string ServiceName = string.Empty;
    public string TwoLetterIsoCode = string.Empty;
    public string Keyword = string.Empty;
    public string Shortcode = string.Empty;

    public static PremiumSubscriptionModel GetModel(MobilePaywallDirect db, AndroidPremiumCustomer customer)
    {
      DirectContainer container = db.LoadContainer(string.Format(@"
        SELECT s.ServiceID, s.Name AS ServiceName, c.TwoLetterIsoCode, sce.Keyword, sce.Shortcode FROM MobilePaywall.core.TemplateServiceInfo AS i
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON i.ServiceID=s.ServiceID
        LEFT OUTER JOIN MobilePaywall.core.Country AS c ON s.FallbackCountryID=c.CountryID
        LEFT OUTER JOIN MobilePaywall.core.ServiceConfiguration AS sc ON s.ServiceConfigurationID=sc.ServiceConfigurationID
        LEFT OUTER JOIN MobilePaywall.core.ServiceConfigurationEntry AS sce ON sce.ServiceConfigurationID=sc.ServiceConfigurationID
		    LEFT OUTER JOIN MobilePaywall.core.AndroidPremiumCustomerServiceMap AS apcsm ON apcsm.ServiceID=s.ServiceID
        WHERE i.IsPremiumSms=1 AND sce.Shortcode != '' AND sce.Keyword != '' AND i.Progress IN (5) 
              AND (apcsm.AndroidPremiumCustomerID IS NULL OR apcsm.AndroidPremiumCustomerID!={0} OR (apcsm.AndroidPremiumCustomerID={0} AND apcsm.PaymentRequestID IS NULL)) AND c.CountryID={1}", customer.ID, customer.Country.ID));

      if (!container.HasValue || container.ColumnCount == 0)
        return null;

      return new PremiumSubscriptionModel()
      {
        ServiceID = container.GetString("ServiceID"),
        ServiceName = container.GetString("ServiceName"),
        TwoLetterIsoCode = container.GetString("TwoLetterIsoCode"),
        Keyword = container.GetString("Keyword"),
        Shortcode = container.GetString("Shortcode")
      };
    }

  }
}