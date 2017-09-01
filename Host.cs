using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniCloud
{
    abstract class Host
    {
        readonly string[] args;
        readonly Runner runner;

        public Host(string[] args, Runner runner)
        {
            this.args = args;
            this.runner = runner;
        }

        public void RunHost()
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

        async Task RunAsync(Job job)
        {
            try
            {
                var result = await runner.RunAsync(job);
                await StoreResultAsync(job, result);
            }
            catch (Exception ex)
            {
                await StoreExceptionAsync(job, ex);
            }
        }

        protected abstract Task StoreResultAsync(Job job, JobResult result);
        protected abstract Task StoreExceptionAsync(Job job, Exception ex);
        protected abstract bool JobCapacityReached();
    }
    
}