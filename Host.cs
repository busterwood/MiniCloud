﻿using System;
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
                job.Started = DateTimeOffset.Now;
                var result = await runner.RunAsync(job);
                result.Finished = DateTimeOffset.Now;
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