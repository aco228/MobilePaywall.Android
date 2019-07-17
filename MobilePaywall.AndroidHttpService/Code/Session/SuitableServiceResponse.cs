using log4net;
using MobilePaywall.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.Session
{
  public class SuitableServiceResponse
  {

    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (SuitableServiceResponse._log == null)
          SuitableServiceResponse._log = LogManager.GetLogger(typeof(SuitableServiceResponse));
        return SuitableServiceResponse._log;
      }
    }
    #endregion

    private Data.Service _serviceData = null;
    private string _name = string.Empty;
    private bool _isPsms = false;
    private string _keyword = string.Empty;
    private string _shortcode = string.Empty;

    public string Name { get { return this._name; } }
    public bool IsPsms { get { return this._isPsms; } }
    public string Keyword { get { return this._keyword; } }
    public string Shortcode { get { return this._shortcode; } }

    public Data.Service ServiceData
    {
      get
      {
        if(this._serviceData != null)
          return this._serviceData;

        int? serviceID = MobilePaywallDirect.Instance.LoadInt(string.Format("SELECT TOP 1 ServiceID FROM MobilePaywall.core.Service WHERE Name='{0}';", this._name));
        if (!serviceID.HasValue)
          return null;

        this._serviceData = Data.Service.CreateManager().Load(serviceID.Value);
        return this._serviceData;          
      }
    }

    public SuitableServiceResponse(string name, bool isPsms)
    {
      this._name = name;
      this._isPsms = isPsms;

      if (this._isPsms)
        this.CollectDataForPremiumSms();
    }

    private void CollectDataForPremiumSms()
    {
      DirectContainer container = MobilePaywallDirect.Instance.LoadContainer(string.Format(@"
        SELECT sce.Keyword, sce.Shortcode FROM MobilePaywall.core.Service AS s
        LEFT OUTER JOIN MobilePaywall.core.ServiceConfigurationEntry AS sce ON s.ServiceConfigurationID=sce.ServiceConfigurationID
        WHERE s.Name='{0}'", this._name));

      if(!container.HasValue)
      {
        this._isPsms = false;
        Log.Error("SuitableService:: Could not load Keyword shortcode for service with name: " + this._name);
        return;
      }

      this._keyword = container.GetString("Keyword");
      this._shortcode = container.GetString("Shortcode");

      if (string.IsNullOrEmpty(this._keyword))
      {
        this._isPsms = false;
        Log.Error("SuitableService:: Keyword is empty for service with name: " + this._name);
        return;
      }

      if (string.IsNullOrEmpty(this._shortcode))
      {
        this._isPsms = false; 
        Log.Error("SuitableService:: Shortcode is empty for service with name: " + this._name);
        return;
      }

    }

  }
}