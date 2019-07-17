using MobilePaywall.Data;
using MobilePaywall.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.Session
{
  public class GetSuitableServices
  {
    private static int DAYS_FOR_TRANSACTION_CHECK = 7;            // days for checking if session has transaction in this days
    private static int DAYS_FOR_PAYMENT_REQUEST_CHECK = 5;        // days for checking if session has payment requests in this days
    private static int DAYS_FOR_ACCESS_CHECK = 1;                 // days for checking if session has accessed service in this days

    /// SUMMARY: See if this customer has made any transaction in past week
    public static bool CheckIfSessionHasTransactionIsPastWeek(AndroidClientSession session)
    {
      int? count = MobilePaywallDirect.Instance.LoadInt(string.Format(@"SELECT COUNT(*) FROM MobilePaywall.core.AndroidClientSession AS acs
        LEFT OUTER JOIN MobilePaywall.core.AndroidClientSessionOLCacheMap AS acmap ON acmap.AndroidClientSessionID=acs.AndroidClientSessionID
        LEFT OUTER JOIN MobilePaywall.core.OLCache AS cache ON cache.OLCacheID=acmap.OLCacheID
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON s.ServiceID=cache.ServiceID
        WHERE acs.AndroidClientSessionID={0} AND cache.IsSubseguent=0 AND cache.TransactionID IS NOT NULL AND cache.SessionCreated >= DATEADD(day,-{1}, GETDATE())", session.ID, DAYS_FOR_TRANSACTION_CHECK));
      return (count.HasValue && count.Value > 0);
    }

    /// SUMMARY: Get suatable services for specific country (Based on ServiceInfo ordered by 'Live and Active' and 'Active but no active')
    ///          in witch this customer has not made any PaymentReqeust
    ///          ONLY WAP PAYMENT
    public static SuitableServiceResponse GetWapService(AndroidClientSession session)
    {
      MobilePaywallDirect db = MobilePaywallDirect.Instance;

      // First we get filter list of all services in which this session has made payment request in past time (7days)
      List<string> servicesWithPaymentRequest = db.LoadContainer(string.Format(@"
        SELECT s.Name FROM MobilePaywall.core.AndroidClientSession AS acs
        LEFT OUTER JOIN MobilePaywall.core.AndroidClientSessionOLCacheMap AS acmap ON acmap.AndroidClientSessionID=acs.AndroidClientSessionID
        LEFT OUTER JOIN MobilePaywall.core.OLCache AS cache ON cache.OLCacheID=acmap.OLCacheID
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON s.ServiceID=cache.ServiceID
        WHERE acs.AndroidClientSessionID={0} AND cache.PaymentRequestID IS NOT NULL AND cache.IsSubseguent=0  AND cache.SessionCreated >= DATEADD(day,-{1}, GETDATE());", session.ID, DAYS_FOR_PAYMENT_REQUEST_CHECK)).GetStringList("Name");

      // Then we filter services in which this session has any access in past day
      List<string> servicesWithAnyAccess = db.LoadContainer(string.Format(@"
        SELECT s.Name FROM MobilePaywall.core.AndroidClientSession AS acs
        LEFT OUTER JOIN MobilePaywall.core.AndroidClientSessionOLCacheMap AS acmap ON acmap.AndroidClientSessionID=acs.AndroidClientSessionID
        LEFT OUTER JOIN MobilePaywall.core.OLCache AS cache ON cache.OLCacheID=acmap.OLCacheID
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON s.ServiceID=cache.ServiceID
        WHERE acs.AndroidClientSessionID={0} AND cache.SessionCreated >= DATEADD(day,-{1}, GETDATE());", session.ID, DAYS_FOR_ACCESS_CHECK)).GetStringList("Name");

      // This is the list of all services that are listed as LIVE AND ACTIVE
      List<string> suitableActiveServices = db.LoadContainer(string.Format(@" 
        SELECT s.Name AS 'Name' FROM MobilePaywall.core.TemplateServiceInfo AS tsi
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON tsi.ServiceID=s.ServiceID
        WHERE s.FallbackCountryID={0} AND tsi.Progress=5 AND tsi.Color = 2 AND IsPremiumSms=0", session.Country.ID)).GetStringList("Name");

      // This is filter list for all services that are listed as NOT LIVE BUT ACTIVE
      List<string> suitableNonActiveServices = db.LoadContainer(string.Format(@" 
        SELECT s.Name AS 'Name' FROM MobilePaywall.core.TemplateServiceInfo AS tsi
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON tsi.ServiceID=s.ServiceID
        WHERE s.FallbackCountryID={0} AND tsi.Progress=5 AND tsi.Color = 1 AND IsPremiumSms=0", session.Country.ID)).GetStringList("Name");

      // randomize active and non active services in one single list
      Random rand = new Random();
      List<string> suitableServices = suitableActiveServices.OrderBy(c => rand.Next()).ToList();
      suitableServices.AddRange(suitableNonActiveServices.OrderBy(c => rand.Next()).ToList());

      foreach (string service in suitableServices)
        if (!servicesWithPaymentRequest.Contains(service) && !servicesWithAnyAccess.Contains(service))
          return new SuitableServiceResponse(service, false);

      return null;
    }

    /// SUMMARY: Get suatable services for specific country (Based on ServiceInfo ordered by 'Live and Active' and 'Active but no active')
    ///          in witch this customer has not made any PaymentReqeust
    ///          ONLY PSMS PAYMENT
    public static SuitableServiceResponse GetPsmsService(AndroidClientSession session)
    {
      MobilePaywallDirect db = MobilePaywallDirect.Instance;

      // First we create filter list for every service in which this session has requested PSMS payment
      List<string> servicesWithPaymentRequest = db.LoadContainer(string.Format(@"
        SELECT s.Name FROM MobilePaywall.core.AndroidPremiumSmsRequest AS psms
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON psms.ServiceID=s.ServiceID
        WHERE AndroidClientSessionID=1 AND psms.Created >= DATEADD(day,-1, GETDATE());", session.ID, DAYS_FOR_PAYMENT_REQUEST_CHECK)).GetStringList("Name");
      
      List<string> suitableActiveServices = db.LoadContainer(string.Format(@" 
        SELECT s.Name AS 'Name' FROM MobilePaywall.core.TemplateServiceInfo AS tsi
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON tsi.ServiceID=s.ServiceID
        WHERE s.FallbackCountryID={0} AND tsi.Progress=5 AND tsi.Color = 2 AND IsPremiumSms=0", session.Country.ID)).GetStringList("Name");

      List<string> suitableNonActiveServices = db.LoadContainer(string.Format(@" 
        SELECT s.Name AS 'Name' FROM MobilePaywall.core.TemplateServiceInfo AS tsi
        LEFT OUTER JOIN MobilePaywall.core.Service AS s ON tsi.ServiceID=s.ServiceID
        WHERE s.FallbackCountryID={0} AND tsi.Progress=5 AND tsi.Color = 1 AND IsPremiumSms=0", session.Country.ID)).GetStringList("Name");

      // randomize active and non active services in one single list
      Random rand = new Random();
      List<string> suitableServices = suitableActiveServices.OrderBy(c => rand.Next()).ToList();
      suitableServices.AddRange(suitableNonActiveServices.OrderBy(c => rand.Next()).ToList());

      foreach (string service in suitableServices)
        if (!servicesWithPaymentRequest.Contains(service))
          return new SuitableServiceResponse(service, true);

      return null;
    }

  }


}