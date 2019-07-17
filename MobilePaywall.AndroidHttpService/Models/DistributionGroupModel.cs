using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Models
{
  public class DistributionGroupModel
  {
    private List<AndroidDistributionGroup>  _distributionGroup;

    public List<AndroidDistributionGroup> DistributionGroups{get { return _distributionGroup; } set { _distributionGroup= value; }}

  }
}