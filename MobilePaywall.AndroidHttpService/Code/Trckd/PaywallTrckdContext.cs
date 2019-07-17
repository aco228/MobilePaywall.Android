using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Trckd.Client;
using Trckd.Client.Web;
using Trckd.Message;

namespace MobilePaywall.AndroidHttpService.Code.Trckd
{
  public class PaywallTrckdContext : TrckdContext
  {
    #region #log#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (PaywallTrckdContext._log == null)
          PaywallTrckdContext._log = LogManager.GetLogger(typeof(PaywallTrckdContext));
        return PaywallTrckdContext._log;
      }
    }
    #endregion

    public override Guid GetDefaultContainerSetID()
    {
      return Guid.Parse("80AC5421-E72D-4E79-9B37-E0CE37247456");

      //IService service = PaywallHttpContext.Current.Service;
      //if (service == null)
      //  return Guid.NewGuid();

      //IContainerManager cManager = Container.CreateManager();
      //Container container = cManager.Load(service.ServiceData);
      //if (container == null)
      //  return Guid.NewGuid();

      //return container.Guid;
    }

    public DeviceInformationResponse Detect()
    {
      DetectClient client = new DetectClient();
      DeviceInformationRequest request = new DeviceInformationRequest(RequestMode.Synchronous, "test", "test", "test", this.FingerprintID.Value, null);
      return client.Detect(request);
    }
  }
}