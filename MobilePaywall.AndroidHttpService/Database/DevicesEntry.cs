using MobilePaywall.AndroidHttpService.Models.Input;
using MobilePaywall.Direct;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Database
{
  public class DevicesEntryManager
  {
    private LoadInputData _data = null;
    private List<DeviceEntry> _result = null;
    private bool _error = false;
    private string _errorMessage = string.Empty;

    public bool Error { get { return this._error; } }
    public string ErrorMessage { get { return this._errorMessage; } }
    public List<DeviceEntry> Result { get { return this._result; } }

    public DevicesEntryManager(LoadInputData data)
    {
      this._data = data;
      this.Validate();
      if (this._error)
        return;

      this._result = new List<DeviceEntry>();
      MobilePaywallDirect db = new MobilePaywallDirect();
      string command = "";

      #region # sql command #

      command =   " SELECT TOP " + data.top + " "
                + " acs.AndroidClientSessionID, "
                + " ad.Name, "
                + " c.TwoLetterIsoCode, "
                + " acs.Device, "
                + " acs.Company, "
                + " acs.Msisdn, "
                + " acs.Created "
                + " FROM MobilePaywall.core.AndroidClientSession AS acs "
                + " LEFT OUTER JOIN MobilePaywall.core.AndroidDistribution AS ad ON acs.AndroidDistributionID=ad.AndroidDistributionID "
                + " LEFT OUTER JOIN MobilePaywall.core.Country AS c ON acs.CountryID=c.CountryID "
                + " WHERE "
                + (!string.IsNullOrEmpty(data.country) ? " c.TwoLetterIsoCode='" + data.country + "' AND " : "")
                + (!string.IsNullOrEmpty(data.appID) && !data.appID.Equals("-1") ? " ad.AndroidDistributionID=" + data.appID + " AND " : "")
                + string.Format(" acs.LastPing >= '{0}' AND acs.LastPing <= '{1}' ", db.Date(this._data.DT_From), db.Date(this._data.DT_To))
                + " ORDER BY acs.LastPing DESC; ";

      #endregion

      DataTable table = db.Load(command);
      if (table == null)
        return;

      foreach (DataRow row in table.Rows)
        this._result.Add(new DeviceEntry(row));
    }

    private void Validate()
    {
      if (string.IsNullOrEmpty(this._data.top))
      {
        this._error = true;
        this._errorMessage = "Top is empty";
        return;
      }
      
      if (string.IsNullOrEmpty(this._data.from))
      {
        this._error = true;
        this._errorMessage = "From is empty";
        return;
      }

      if (string.IsNullOrEmpty(this._data.to))
      {
        this._error = true;
        this._errorMessage = "To is empty";
        return;
      }

      if (string.IsNullOrEmpty(this._data.appID))
      {
        this._error = true;
        this._errorMessage = "AppID is empty";
        return;
      }

      DateTime temp;
      if (!DateTime.TryParse(this._data.from, out temp))
      {
        this._error = true;
        this._errorMessage = "From could not be parsed";
        return;
      }
      else
        this._data.DT_From = temp;
      if (!DateTime.TryParse(this._data.to, out temp))
      {
        this._error = true;
        this._errorMessage = "To could not be parsed";
        return;
      }
      else
        this._data.DT_To = temp;
    }

  }

  public class DeviceEntry
  {
    public string AndroidClientSessionID { get; set; }
    public string ApplicationName { get; set; }
    public string CountryCode { get; set; }
    public string DeviceName { get; set; }
    public string Company { get; set; }
    public string Msisdn { get; set; }
    public string Created { get; set; }

    public DeviceEntry(DataRow row)
    {
      this.AndroidClientSessionID = row[(int)DeviceEntry.Columns.AndroidClientSessionID].ToString();
      this.ApplicationName = row[(int)DeviceEntry.Columns.ApplicationName].ToString();
      this.CountryCode = row[(int)DeviceEntry.Columns.CountryCode].ToString();
      this.DeviceName = row[(int)DeviceEntry.Columns.DeviceName].ToString();
      this.Company = row[(int)DeviceEntry.Columns.Company].ToString();
      this.Msisdn = row[(int)DeviceEntry.Columns.Msisdn].ToString();
      this.Created = row[(int)DeviceEntry.Columns.Created].ToString();
    }

    public enum Columns
    {
      AndroidClientSessionID,
      ApplicationName,
      CountryCode,
      DeviceName,
      Company,
      Msisdn,
      Created
    }
    
  }
}