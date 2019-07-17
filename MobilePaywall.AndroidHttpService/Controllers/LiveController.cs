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

    public ActionResult Index()
    {
      LiveIndexModel model = new LiveIndexModel();
      return View(model);
    }

    public ActionResult CreateDistribution(string id)
    {
      int _gid = -1;
      if (!Int32.TryParse(id, out _gid))
        return this.Content("Could not parse id");

      AndroidDistributionGroup group = AndroidDistributionGroup.CreateManager().Load(_gid);
      if (group == null)
        return this.Content("Group is null");

      CreateDistributionModel model = new CreateDistributionModel();
      model.Group = group;
      return View(model);
    }

    public ActionResult CreateDistributionGroup()
    {
      return View();
    }

    public ActionResult AllDistribution(string id)
    {
      LiveDistributionModel model = new LiveDistributionModel();

      Data.AndroidDistributionGroup adg = Data.AndroidDistributionGroup.CreateManager().Load(int.Parse(id));
      List<Data.AndroidDistribution> androidDist= Data.AndroidDistribution.CreateManager().Load(adg);
      
      model.AndroidDistributions = androidDist;
      model.AndroidGroup = adg;
      return View(model);
    }

    public FileResult DistributionLogo(string id)
    {
      int _id = -1;
      if (!Int32.TryParse(id, out _id))
        return null;

      AndroidDistribution dis = AndroidDistribution.CreateManager().Load(_id);
      if (dis == null)
        return null;

      AndroidDistributionLogo logo = AndroidDistributionLogo.CreateManager().Load(dis);
      if (logo == null)
        return null;

      return new FileContentResult(logo.Data, "image/jpeg");
    }

    public ActionResult Distribution(string id)
    {
      Data.AndroidDistribution model = AndroidDistribution.CreateManager().Load(int.Parse(id));
      if (model == null)
        return this.Json(new { status= false, message = "Could not parse AndroidDistribution ID"});
      return View(model);
    }

    public ActionResult DistributionGroup() {
      DistributionGroupModel model = new DistributionGroupModel();
      model.DistributionGroups = AndroidDistributionGroup.CreateManager().Load();

      if (model == null)
        return this.Content("AndroidDistributionGroup is null");
      return View(model); 
    }

    public ActionResult OneDistributionGroup(string id)
    {
      Data.AndroidDistributionGroup model = AndroidDistributionGroup.CreateManager().Load(int.Parse(id));
      if (model == null)
        return this.Content("AndroidDistributionGroup is null");
      return View("~/Views/Live/OneDistributionGroup.cshtml", model);
    }

    public ActionResult GetDevices(LoadInputData data)
    {
      DevicesEntryManager manager = new DevicesEntryManager(data);
      if(manager.Error)
        return this.Json(new { status = false, message = manager.ErrorMessage });
      return PartialView("~/Views/Live/Partials/_Devices.cshtml", manager.Result);
    }

    public ActionResult Device(string sid)
    {
      int androidSessionID = -1;
      if (!Int32.TryParse(sid, out androidSessionID))
        return this.Content("could not parse sid");

      AndroidClientSession session = AndroidClientSession.CreateManager().Load(androidSessionID);
      if (session == null)
        return this.Content("There is no session with this id");

      List<AndroidClientLog> logs = AndroidClientLog.CreateManager().Load(session);
      return View("~/Views/Live/Device.cshtml", new LiveDeviceModel(session, logs));
    }

    public ActionResult Send(string value, string token)
    {
      return this.FirebaseSend(value, token, null);
    }


  }
}