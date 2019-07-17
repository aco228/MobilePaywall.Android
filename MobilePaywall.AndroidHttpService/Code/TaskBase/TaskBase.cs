using log4net;
using MobilePaywall.AndroidHttpService.Hubs;
using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.TaskBase
{
  public abstract class TaskBase : ITask
  {

    #region #logging#
    private static ILog _log = null;

    protected static ILog Log
    {
      get
      {
        if (TaskBase._log == null)
          TaskBase._log = LogManager.GetLogger(typeof(TaskBase));
        return TaskBase._log;
      }
    }
    #endregion

    private AndroidClientSession _session = null;
    private string _name = string.Empty;
    private Guid _identifier;
    private DateTime _executionTime;
    private bool _isExecuted = false;
    private bool _remain = false;

    public string Name { get { return this._name; } protected set { this._name = value; } }
    public Guid Identifier { get { return this._identifier; } }
    public DateTime ExecutionTime { get { return this._executionTime; } protected set { this._executionTime = value; } }
    public bool IsExecuted { get { return this._isExecuted; } protected set { this._isExecuted = value; } }
    public bool Remain { get { return this._remain; } protected set { this._remain = value; } }
    public AndroidClientSession AndroidClientSession { get { return this._session; } protected set { this._session = value; } }

    public TaskBase(string name, string sessionID, DateTime executionTime)
    {
      int _sessionID = -1;
      if (!Int32.TryParse(sessionID, out _sessionID))
      {
        Log.Error(string.Format("PayTask:: Could not parse sessionID (sessionID='{0})", sessionID));
        this.IsExecuted = true;
        return;
      }

      this._session = AndroidClientSession.CreateManager().Load(_sessionID);
      if (this._session == null)
      {
        Log.Error("PayTask:: Could not load session with id:" + _sessionID);
        this.IsExecuted = true;
        return;
      }

      this._name = name;
      this._identifier = Guid.NewGuid();
      this._executionTime = executionTime;
      Log.Debug("Task '" + name + "' is set to be executed in " + this._executionTime.ToString());
    }

    protected abstract TaskExecutionResult TaskLogic();

    public void Initialize()
    {
      if(this._session == null)
      {
        this._isExecuted = true;
        return;
      }

      this.InitializeTask();
    }

    protected abstract void InitializeTask();

    public void ExecuteTask()
    {
      if (this._isExecuted)
        return;

      try
      {
        TaskExecutionResult result = this.TaskLogic();

        this._isExecuted = true;
        if (result.RepeatExecution)
          this._isExecuted = false;
        if (result.NewExecutionTime.HasValue)
          this._executionTime = result.NewExecutionTime.Value;

        Log.Debug("Task '" + this._name + "' is executed ");
      }
      catch(Exception e)
      {
        Log.Error("TASK.'" + this._name + "'.FATAL", e);
      }
    }
    
    public void LogSession(string text)
    {
      AndroidClientLog.Log(this.AndroidClientSession.ID.ToString(), this.Name, text);
      LiveDeviceHub.Current.Update(this.AndroidClientSession.ID.ToString(), this.Name, text, true, DateTime.Now);
    }

  }
}