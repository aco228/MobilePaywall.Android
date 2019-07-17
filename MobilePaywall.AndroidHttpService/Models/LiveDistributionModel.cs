using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MobilePaywall.AndroidHttpService.Models
{
  public class LiveDistributionModel
  {
    private List<Data.AndroidDistribution> _androidDistributions;
    private List<Data.AndroidDistributionLogo> _androidDistributionLogos;
    private List<FileContentResult> _files;
    private Dictionary<FileContentResult, int> _logoData;
    private Data.AndroidDistributionGroup _androidGroup;

    public List<Data.AndroidDistribution> AndroidDistributions{ get { return _androidDistributions; } set {this._androidDistributions = value; }}
    public List<Data.AndroidDistributionLogo> AndroidDistributionLogos { get { return _androidDistributionLogos; }set { this._androidDistributionLogos = value; } }
    public List<FileContentResult> Files { get { return _files; }set { this._files = value; } }
    public Dictionary<FileContentResult, int> LogoData { get { return this._logoData; }set { this._logoData = value; } }
    public Data.AndroidDistributionGroup AndroidGroup { get { return this._androidGroup; } set { this._androidGroup = value; } }
  }
}