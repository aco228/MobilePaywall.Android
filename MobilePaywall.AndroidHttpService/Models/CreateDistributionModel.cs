using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Models
{
  public class CreateDistributionModel
  {
    private AndroidDistributionGroup _group = null;

    public AndroidDistributionGroup Group { get { return this._group; } set { this._group = value; } }
  }
}