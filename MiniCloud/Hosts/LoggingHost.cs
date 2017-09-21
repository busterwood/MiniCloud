using System;
using System.Threading.Tasks;

namespace MiniCloud
{
    abstract class LoggingHost : Host
    {
        readonly Runner runner;

        public LoggingHost(string[] args, Runner runner): base (args)
        {
            this.runner = runner;
        }

        public override void RunHost()
        {
            Log.Info("Starting");
            base.RunHost();
        }

        protected override Task<JobResult> RunAsyncCore(Job job)
        {
            Log.Info($"Running {job}");
            return runner.RunAsync(job);
        }

        protected override Task StoreExceptionAsync(Job job, Exception ex)
        {
            Log.Info($"Caught exception running {job}: {ex}");
            return Tasks.Complete;
        }

        protected override Task StoreResultAsync(Job job, JobResult result)
        {
            Log.Info($"{job} exited code {result}", new { ElsapedMS = (DateTimeOffset.UtcNow - job.Started).TotalMilliseconds } );
            return Tasks.Complete;
        }
    }
}