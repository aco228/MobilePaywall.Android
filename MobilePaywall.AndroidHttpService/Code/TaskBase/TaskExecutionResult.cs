using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.TaskBase
{
  public class TaskExecutionResult
  {
    private bool _repeatExecution = false;
    private DateTime? _newExecutionTime;

    public bool RepeatExecution { get { return this._repeatExecution; } set { this._repeatExecution = value; } }
    public DateTime? NewExecutionTime { get { return this._newExecutionTime; } set { this._newExecutionTime = value; } }

    public TaskExecutionResult()
    {

    }
  }
}