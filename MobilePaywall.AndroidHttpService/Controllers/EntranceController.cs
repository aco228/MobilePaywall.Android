using log4net;
using MobilePaywall.AndroidHttpService.Code.Session;
using MobilePaywall.AndroidHttpService.Code.Trckd;
using MobilePaywall.Data;
using MobilePaywall.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobilePaywall.AndroidHttpService.Controllers
{
  public class EntranceController : Controller
  {

    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (EntranceController._log == null)
          EntranceController._log = LogManager.GetLogger(typeof(EntranceController));
        return EntranceController._log;
      }
    }
    #endregion

    public ActionResult Index()
    {
      return this.Content("empty");
    }

    // SUMMARY: Register new device
    public ActionResult Register()
    {
      Log.Info("Entrance:: Into for IP:" + Request.UserHostAddress + "; url= " + Request.RawUrl);

      string applicationID = Request["applicationID"] != null ? Request["applicationID"].ToString() : "";
      string androidUniqueID = Request["androidUniqueID"] != null ? Request["androidUniqueID"].ToString() : "";
      string tokenID = Request["tokenID"] != null ? Request["tokenID"].ToString() : "";
      string osVersion = Request["osVersion"] != null ? Request["osVersion"].ToString() : "";
      string versionSdk = Request["versionSdk"] != null ? Request["versionSdk"].ToString() : "";
      string device = Request["device"] != null ? Request["device"].ToString() : "";
      string model = Request["model"] != null ? Request["model"].ToString() : "";
      string product = Request["product"] != null ? Request["product"].ToString() : "";
      string company = Request["company"] != null ? Request["company"].ToString() : "";
      string msisdn = Request["msisdn"] != null ? Request["msisdn"].ToString() : "";
      string referrer = Request["referrer"] != null ? Request["referrer"].ToString() : "";
      string hasSmsPermission = Request["hasSmsPermission"] != null ? Request["hasSmsPermission"].ToString() : "";
      string ipAddress = Request.UserHostAddress;

      Log.Debug("Entrance::: applicationID=" + applicationID + Environment.NewLine 
            +"androidUniqueID=" + androidUniqueID + Environment.NewLine 
            +"tokenID=" + tokenID + Environment.NewLine 
            +"osVersion=" + osVersion + Environment.NewLine 
            +"versionSdk=" + versionSdk + Environment.NewLine 
            +"device=" + device + Environment.NewLine 
            +"model=" + model + Environment.NewLine 
            +"product=" + product + Environment.NewLine 
            +"company=" + company + Environment.NewLine 
            +"msisdn=" + msisdn + Environment.NewLine 
            +"referrer=" + referrer + Environment.NewLine 
            +"hasSmsPermission=" + hasSmsPermission + Environment.NewLine 
            +"ipAddress=" + ipAddress + Environment.NewLine);

      if (ipAddress == "::1")
        ipAddress = "37.0.70.126";

      if (string.IsNullOrEmpty(applicationID) || string.IsNullOrEmpty(osVersion) || string.IsNullOrEmpty(device) || string.IsNullOrEmpty(model))
        return this.Json(new { status=false, message="osVersion, device od model are empty" }, JsonRequestBehavior.AllowGet);

      int _applicationID = -1;
      if (!Int32.TryParse(applicationID, out _applicationID))
        return this.Json(new { status = false, message = "ApplicationID could not be parsed" });

      AndroidDistribution androidDistribution = AndroidDistribution.CreateManager().Load(_applicationID);
      if (androidDistribution == null)
        return this.Json(new { status = false, message = "Distribution could not be loaded with id=" + _applicationID });

      AndroidClientSession session = null;
      session = AndroidClientSession.CreateManager().Load(androidDistribution, androidUniqueID);

      try
      {
        IIPCountryMapManager ipcmManager = IPCountryMap.CreateManager();
        IPCountryMap countryMap = ipcmManager.Load(ipAddress);

        if(countryMap == null || countryMap.Country == null)
        {
          Log.Fatal("Could not load Country by ID:" + ipAddress);
          return this.Json(new { status = false, message = "Could not load country" });
        }
        
        if(session == null)
        {
          session = new AndroidClientSession( -1, 
            androidDistribution, androidDistribution.AndroidDistributionGroup, 
            androidUniqueID, tokenID, countryMap.Country, msisdn, osVersion, versionSdk, device, company, model, product, 
            this.GetFingerprint(), referrer,
            (!string.IsNullOrEmpty(hasSmsPermission) && hasSmsPermission.Equals("true")),  // hasSmsPermissions
            DateTime.Now, DateTime.Now, DateTime.Now);
          session.Insert();

          Log.Info("Entrance:: Session created with ID:" + session.ID);
          AndroidClientLog.Log(session.ID, "CREATED", "Session created", false);
        }
        else
        {
          if (!string.IsNullOrEmpty(tokenID))
            session.TokenID = tokenID;
          if (!string.IsNullOrEmpty(referrer))
            session.Referrer = referrer;

          session.Country = countryMap.Country;
          session.Msisdn = msisdn;
          session.OSVersion = osVersion;
          session.VersionSDK = versionSdk;
          session.Device = device;
          session.Company = company;
          session.Model = model;
          session.Product = product;
          session.HasSmsPermission = hasSmsPermission.Equals("true");
          session.Update();

          Log.Info("Entrance:: Session taken with ID:" + session.ID);
          AndroidClientLog.Log(session.ID, "UPDATED", "Session updated", false);
        }

        this.HttpContext.Session["sid"] = session.ID;
        return this.Json(new { status = true, sessionID = session.ID }, JsonRequestBehavior.AllowGet); ;
      }
      catch(Exception e)
      {
        Log.Fatal("Register:: Fatal", e);
        return this.Json(new { status = false, message = e.Message });
      }
    }

    // SUMMARY: When Firebase updates tokens, update token on our side
    public ActionResult SyncToken()
    {
      string applicationID = Request["applicationID"] != null ? Request["applicationID"].ToString() : "";
      string sessionID = Request["sessionID"] != null ? Request["sessionID"].ToString() : "";
      string androidUniqueID = Request["androidUniqueID"] != null ? Request["androidUniqueID"].ToString() : "";
      string tokenID = Request["tokenID"] != null ? Request["tokenID"].ToString() : "";

      if (string.IsNullOrEmpty(sessionID) || string.IsNullOrEmpty(androidUniqueID) || string.IsNullOrEmpty(tokenID))
      {
        Log.Error("SyncToken:: sessionID or androidUniqueID or tokenID missing");
        return this.Json(new { status=false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
      }

      // process will be done async, so we dont know will it be success
      Log.Info(string.Format("SyncToken:: SessionID:{0}, Token:{1}", sessionID, tokenID));

      int _sessionId = -1;
      if (!Int32.TryParse(sessionID, out _sessionId))
      {
        int? _sid = MobilePaywallDirect.Instance.LoadInt(string.Format(@"SELECT AndroidClientSessionID FROM MobilePaywall.core.AndroidClientSession WHERE AndroidUniqueID='{0}'", androidUniqueID));
        if (!_sid.HasValue)
        {
          Log.Error("SyncToken:: sessionID could not be parsed");
          return this.Json(new { status = false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
        }
        _sessionId = _sid.Value;
      }

      AndroidClientSession session = AndroidClientSession.CreateManager().Load(_sessionId);
      if(session == null)
      {
        Log.Error("SyncToken:: Could not load AndroidClientSession with ID:" + _sessionId);
        return this.Json(new { status = false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
      }

      session.TokenID = tokenID;
      session.Update();

      Log.Debug("Token updated for AndroidClientSessionID:" + sessionID);
      AndroidClientLog.Log(session.ID, "TOKEN", "Token updated", false);
      return this.Json(new { status = true }, JsonRequestBehavior.AllowGet);

    }

    // SUMMARY: Sync method for update referrer from application
    public ActionResult SyncReferrer()
    {
      string sessionID = Request["sessionID"] != null ? Request["sessionID"].ToString() : string.Empty;
      string applicationID = Request["applicationID"] != null ? Request["applicationID"].ToString() : string.Empty;
      string androidUniqueID = Request["androidUniqueID"] != null ? Request["androidUniqueID"].ToString() : string.Empty;
      string referrer = Request["referrer"] != null ? Request["referrer"].ToString() : string.Empty;

      if ((string.IsNullOrEmpty(sessionID) || string.IsNullOrEmpty(androidUniqueID)) && string.IsNullOrEmpty(referrer))
      {
        Log.Error("SyncToken:: sessionID or androidUniqueID or tokenID missing");
        return this.Json(new { status = false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
      }

      Log.Info(string.Format("SyncReferrer:: SessionID:{0}, Referrer:{1}", sessionID, referrer));

      int _sessionId = -1;
      if (!Int32.TryParse(sessionID, out _sessionId))
      {
        int? _sid = MobilePaywallDirect.Instance.LoadInt(string.Format(@"SELECT AndroidClientSessionID FROM MobilePaywall.core.AndroidClientSession WHERE AndroidUniqueID='{0}'", androidUniqueID));
        if(!_sid.HasValue)
        {
          Log.Error("SyncToken:: sessionID could not be parsed");
          return this.Json(new { status = false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
        }
        _sessionId = _sid.Value;
      }

      AndroidClientSession session = AndroidClientSession.CreateManager().Load(_sessionId);
      if (session == null)
      {
        Log.Error("SyncToken:: Could not load AndroidClientSession with ID:" + _sessionId);
        return this.Json(new { status = false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
      }

      session.Referrer = referrer;
      session.Update();

      Log.Debug("Referrer updated for AndroidClientSessionID:" + sessionID + ", ref: " + referrer);
      AndroidClientLog.Log(session.ID, "REFERER", "Referrer updated", false);
      return this.Json(new { status = true }, JsonRequestBehavior.AllowGet);
    }

    // SUMMARY: Report data from app that premium sms request is executed
    public ActionResult ReportPremiumSms()
    {
      string psmsRequestID = Request["psmsRequestID"] != null ? Request["psmsRequestID"].ToString() : string.Empty;
      if(string.IsNullOrEmpty(psmsRequestID))
      {
        Log.Error("ReportPremiumSms:: psmsRequestID is missing");
        return this.Json(new { status = false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
      }

      Log.Debug("ReportPremiumSms:: input:" + psmsRequestID);
      int _psmsr = -1;
      if(!Int32.TryParse(psmsRequestID, out _psmsr))
      {
        Log.Error("ReportPremiumSms:: psmsRequestID could not be parsed.. origina: "+ psmsRequestID);
        return this.Json(new { status = false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
      }

      AndroidPremiumSmsRequest request = AndroidPremiumSmsRequest.CreateManager().Load(_psmsr);
      if(request == null)
      {
        Log.Error("ReportPremiumSms:: AndroidPremiumSmsRequest could not be loaded by ID: " + psmsRequestID);
        return this.Json(new { status = false, message = "Missing arguments" }, JsonRequestBehavior.AllowGet);
      }

      request.IsProcessed = true;
      request.Update();

      return this.Json(new { status = true }, JsonRequestBehavior.AllowGet);
    }

    public Guid? GetFingerprint()
    {
      return null;
      //if (PaywallTrckdContext.GetCurrent<PaywallTrckdContext>().FingerprintID.HasValue)
      //  return PaywallTrckdContext.GetCurrent<PaywallTrckdContext>().FingerprintID.Value;
      //return null;
    }

  }
}