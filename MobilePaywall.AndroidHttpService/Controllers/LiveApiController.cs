using MobilePaywall.AndroidHttpService.Models;
using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using MobilePaywall.AndroidHttpService.Hubs;
using MobilePaywall.AndroidHttpService.Models.Input;
using MobilePaywall.AndroidHttpService.Database;

namespace MobilePaywall.AndroidHttpService.Controllers
{
  public partial class LiveController : Controller
  {
    
    public ActionResult FirebaseSend(string message, string tokenID, int? sessionID)
    {
      try
      {
        //string SERVER_API_KEY = "AIzaSyD8bOTI-Fqj0GWenzzTDkX4HbncV1zqjlc";
        string SERVER_API_KEY = "AIzaSyCAgttSrkMqP98AXNruAVtBjX9D81wqe_I";
        var value = message;
        WebRequest tRequest;
        tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
        tRequest.Method = "post";
        tRequest.Headers.Add(string.Format("Authorization: key={0}", SERVER_API_KEY));

        tRequest.ContentType = "application/json";
        var data = new
        {
          to = tokenID,
          data = new { message = message }
        };

        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(data);
        Byte[] byteArray = Encoding.UTF8.GetBytes(json);
        tRequest.ContentLength = byteArray.Length;

        Stream dataStream = tRequest.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);
        dataStream.Close();

        WebResponse tResponse = tRequest.GetResponse();
        dataStream = tResponse.GetResponseStream();
        StreamReader tReader = new StreamReader(dataStream);
        String sResponseFromServer = tReader.ReadToEnd();

        tReader.Close();
        dataStream.Close();
        tResponse.Close();

        if (sessionID.HasValue)
        {
          AndroidClientLog.Log(sessionID.Value, "SEND", message, false);
          LiveDeviceHub.Current.Update(sessionID.Value.ToString(), "SEND", message, false, DateTime.Now);
        }

        return this.Json(new { status = true }, JsonRequestBehavior.AllowGet);
      }
      catch (Exception e)
      {
        return this.Json(new { status = false }, JsonRequestBehavior.AllowGet);
      }
    }

    [HttpPost]
    public ActionResult ApiUpdateDistributionGroup(string id, string name, string description)
    {
      AndroidDistributionGroup androidDistributionGroup = AndroidDistributionGroup.CreateManager().Load(int.Parse(id));
      if (androidDistributionGroup == null)
        return this.Json(new { status = false, message = " Error parsing Distribution Group" });
      androidDistributionGroup.Name = name;
      androidDistributionGroup.Description = description;
      androidDistributionGroup.Update();
      return this.Json(new { status = true, message = "DistributionGroup is updated with values " + "Name: " + name + ", Description: " + description }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult ApiUpdateDistribution(string id, string name, string description)
    {
      AndroidDistribution androidDistribution = AndroidDistribution.CreateManager().Load(int.Parse(id));
      if (androidDistribution == null)
        return this.Json(new { status = false, message = " Error parsing Distribution" });

      androidDistribution.Name = name;
      androidDistribution.Description = description;
      androidDistribution.Update();
      return RedirectToAction("AllDistribution", "Live", new { id = androidDistribution.AndroidDistributionGroup.ID });
      //  return this.Json(new { status = true, message = "Distribution is updated with values " + "Name: " + name + ", Description: " + description }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult ApiDeleteDistributionGroup(string id)
    {
      Data.AndroidDistributionGroup distributionGroup = AndroidDistributionGroup.CreateManager().Load(int.Parse(id));
      List<Data.AndroidDistribution> distributions = Data.AndroidDistribution.CreateManager().Load(distributionGroup);
      foreach (Data.AndroidDistribution dist in distributions)
      {
        dist.Delete();
      }

      distributionGroup.Delete();
      return RedirectToAction("DistributionGroup", "Live");
      }

    public ActionResult ApiDeleteDistribution(string id)
    {
      Data.AndroidDistribution distribution = AndroidDistribution.CreateManager().Load(int.Parse(id));
      distribution.Delete();

      return RedirectToAction("AllDistribution", "Live", new { id = distribution.AndroidDistributionGroup.ID });
    }

    [HttpPost]
    public ActionResult ApiCreateDistributionGroup(string name, string description)
    {
      AndroidDistributionGroup androidDistributionGroup = new AndroidDistributionGroup(-1, name, description, DateTime.Now, DateTime.Now);
      androidDistributionGroup.Insert();
      return this.Json(new { status = true, message = "Distribution group is created.." });
    }

    [HttpPost]
    public ActionResult ApiCreateDistribution(string name, string groupID, string description)
    {
      AndroidDistributionGroup androidDistributionGroup = AndroidDistributionGroup.CreateManager().Load(int.Parse(groupID));
      AndroidDistribution androidDistribution = new AndroidDistribution(-1, androidDistributionGroup, name, description, DateTime.Now, DateTime.Now);
      androidDistribution.Insert();
      return RedirectToAction("AllDistribution", "Live", new { id = androidDistributionGroup.ID });
      //return this.Json(new { status = true, message = "Distribution is created"});
    }

    public ActionResult ApiUpdateLogo(string distributionID, HttpPostedFileBase file)
    {
      if (file == null && file.ContentLength == 0)
        return this.Json(new { status = false, message = "nemmaaa" });
      if (!file.FileName.Contains(".png"))
        return this.Json(new { status = false, message = "Provide  .png" });

      byte[] data = null;
      using (MemoryStream ms = new MemoryStream())
      {
        file.InputStream.CopyTo(ms);
        data = ms.GetBuffer();
      }

      if (data == null)
        return this.Json(new { status = false, message = "Data is null" });
      AndroidDistribution androidDistribution = AndroidDistribution.CreateManager().Load(int.Parse(distributionID));
      AndroidDistributionLogo androidDistributionLogo = AndroidDistributionLogo.LoadByDistribution(androidDistribution);

      if (androidDistributionLogo == null)
      {
        androidDistributionLogo = new AndroidDistributionLogo(-1, androidDistribution, data, true, DateTime.Now, DateTime.Now);
        androidDistributionLogo.Insert();
        return RedirectToAction("AllDistribution", "Live", new { id = androidDistribution.AndroidDistributionGroup.ID });
      }
      else
      {
        androidDistributionLogo.Data = data;
        androidDistributionLogo.Update();
        return RedirectToAction("AllDistribution", "Live", new { id = androidDistribution.AndroidDistributionGroup.ID });
      //  return RedirectToAction("Index", "Live");
      }
    }


  }
}