using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCache.Specs
{
    public class DeterministicTaskScheduler : TaskScheduler
    {
        private readonly List<Task> scheduledTasks = new List<Task>();

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return scheduledTasks;
        }

        protected override void QueueTask(Task task)
        {
            scheduledTasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            scheduledTasks.Add(task);
            return false;
        }

        public override int MaximumConcurrencyLevel { get { return 1; } }

        public void RunPendingTasks()
        {
            foreach (var task in scheduledTasks.ToArray())
            {
                TryExecuteTask(task);

                // Propagate exceptions
                try
                {
                    task.Wait();
                }
                catch (AggregateException aggregateException)
                {
                    throw aggregateException.InnerException;
                }
                finally
                {
                    scheduledTasks.Remove(task);
                }
            }
        }

        public void RunTasksUntilIdle()
        {
            while(scheduledTasks.Any())
            {
                this.RunPendingTasks();
            }
        }
    }
}
