using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.TaskBase
{
  public interface ITask
  {
    string Name { get; }
    Guid Identifier { get; }
    DateTime ExecutionTime { get; }
    bool IsExecuted { get; }
    bool Remain { get; }
    AndroidClientSession AndroidClientSession { get; }

    void Initialize();
    void ExecuteTask();
    void LogSession(string text);

  }
}