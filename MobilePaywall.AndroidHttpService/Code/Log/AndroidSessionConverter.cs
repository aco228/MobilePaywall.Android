using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.Log
{
  public class AndroidSessionConverter : PatternLayoutConverter
  {
    
    protected override void Convert(TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
    {
      try
      {
        string sid = HttpContext.Current.Session["sid"] != null ? HttpContext.Current.Session["sid"].ToString() : "0";
        writer.Write(sid);
      }
      catch (Exception e)
      {
        writer.Write("0");
      }
    }

  }
}