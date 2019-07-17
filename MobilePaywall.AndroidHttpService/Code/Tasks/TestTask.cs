using MobilePaywall.AndroidHttpService.Controllers;
using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.Tasks
{
  public class TestTask : TaskBase.TaskBase
  {
    public TestTask(string sessionID )
      :base("TestTask", sessionID, DateTime.Now.AddMinutes(1))
    { }

    protected override void InitializeTask()
    { }

    protected override TaskBase.TaskExecutionResult TaskLogic()
    {
      LiveController controller = new LiveController();
      controller.FirebaseSend("aco", this.AndroidClientSession.TokenID, this.AndroidClientSession.ID);

      return new TaskBase.TaskExecutionResult();
    }


  }
}