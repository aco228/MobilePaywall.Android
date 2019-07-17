using MobilePaywall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace MobilePaywall.AndroidHttpService.Code.TaskBase
{
  public class TaskManager
  {
    private List<ITask> _tasks = null;

    public List<ITask> Tasks { get { return this._tasks; } }
    public List<AndroidClientSession> Sessions { get { return (from t in this._tasks where !t.IsExecuted select t.AndroidClientSession).ToList(); } }

    public TaskManager()
    {
      this._tasks = new List<ITask>();
    }

    public void AddTask(ITask task)
    {
      // check if this session allready has task waiting
      if ((from s in this.Sessions where s.ID == task.AndroidClientSession.ID select s).FirstOrDefault() != null)
        return;
      
      task.Initialize();
      this._tasks.Add(task);
    }

    public void Run()
    {
      for(;;)
      {
        for (int i = 0; i < this._tasks.Count; i++ )
        {
          ITask task = this._tasks.ElementAt(i);

          if (task.IsExecuted && !task.Remain)
          {
            this._tasks.RemoveAt(i);
            continue;
          }

          if(DateTime.Now.CompareTo(task.ExecutionTime) < 0)
            continue;

          task.ExecuteTask();

          if (task.IsExecuted && !task.Remain)
            this._tasks.RemoveAt(i);
        }

        Thread.Sleep(250);
      }
    }

  }
}