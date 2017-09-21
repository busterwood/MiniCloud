using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniCloud
{
    /// <summary>
    /// A host that can run a number of jobs in parallel
    /// </summary>
    abstract class Host
    {
        readonly string[] args;

        public Host(string[] args)
        {
            this.args = args;
        }

        public virtual void RunHost()
        {
            var jobComplete = new AutoResetEvent(false);
            for (;;)
            {
                foreach (var j in WaitForJobs())
                {
                    var ignored = RunAsync(j).ContinueWith(t => jobComplete.Set());
                }

                while (JobCapacityReached())
                {
                    jobComplete.WaitOne();
                }
            }
        }

        protected abstract List<Job> WaitForJobs();

        protected virtual async Task RunAsync(Job job)
        {
            try
            {
                var result = await RunAsyncCore(job);
                await StoreResultAsync(job, result);
            }
            catch (Exception ex)
            {
                await StoreExceptionAsync(job, ex);
            }
        }

        protected abstract Task<JobResult> RunAsyncCore(Job job);
        protected abstract Task StoreResultAsync(Job job, JobResult result);
        protected abstract Task StoreExceptionAsync(Job job, Exception ex);
        protected abstract bool JobCapacityReached();
    }
}