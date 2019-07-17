using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Models
{
  public class LiveIndexModel : ModelBase
  {
    private List<AndroidDistribution> _androidDistributions = null;

    public List<AndroidDistribution> Distributions { get { return this._androidDistributions; } }

    public LiveIndexModel()
    {
      this._androidDistributions = AndroidDistribution.CreateManager().Load();
    }

  }
}