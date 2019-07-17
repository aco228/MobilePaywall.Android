using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Models
{
  public class CountryModel
  {
    public int ID { get; set; }
    public string GlobalName { get; set; }
    
    public CountryModel(int id, string name)
    {
      this.ID = id;
      this.GlobalName = name;
    }
  }
}