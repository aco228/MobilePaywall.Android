using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Models.Input
{
  public class LoadInputData
  {
    public string top { get; set; }
    public string country { get; set; }
    public string appID { get; set; }
    public string from { get; set; }
    public string to { get; set; }

    public DateTime DT_From { get; set; }
    public DateTime DT_To { get; set; }
  }
}