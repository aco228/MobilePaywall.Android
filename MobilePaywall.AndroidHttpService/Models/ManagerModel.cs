using MobilePaywall.Data;
using MobilePaywall.Direct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Models
{
  public class ManagerModel
  {
    private List<AndroidPremiumCustomer> _customers = null;
    private List<CountryModel> _countries = null;
    public List<AndroidPremiumCustomer> Customers { get { return this._customers; } set { this._customers = value; } }
    public List<CountryModel> Countries { get { return this._countries; } }
    public ManagerModel()
    {
      this._countries = new List<CountryModel>();
      MobilePaywallDirect md = new MobilePaywallDirect();
      DirectContainer container = md.LoadContainer(string.Format(@"SELECT DISTINCT c.CountryID,c.GlobalName FROM MobilePaywall.core.Country c 
                                                                    JOIN MobilePaywall.core.AndroidPremiumCustomer AS a ON c.CountryID = a.CountryID"));
      foreach(DirectContainerRow row in container.Rows)
      {
        int? id = row.GetInt("CountryID");
        string name = row.GetString("GlobalName");

        CountryModel c = new CountryModel(id.Value, name);
        this.Countries.Add(c);
      }

      if (container == null)
        this._countries = new List<CountryModel>();

      this._customers = new List<AndroidPremiumCustomer>();
    }


  }

}