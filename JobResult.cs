using System.IO;

namespace MiniCloud
{
    internal class JobResult
    {
        public Job Job { get; set; }

        public JobResult(Job job)
        {
            this.Job = job;
        }

        public int ExitCode { get; internal set; }
        public Stream Logging { get; internal set; }
        public Stream Output { get; internal set; }
    }
}