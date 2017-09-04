using System;
using System.IO;

namespace MiniCloud
{
    internal class JobResult
    {
        public Job Job { get; set; }

        public JobResult(Job job)
        {
            Job = job;
            Logging = new MemoryStream();
            Output = new MemoryStream();
        }

        public int ExitCode { get; set; }
        public Stream Logging { get; set; }
        public Stream Output { get; set; }
        public DateTimeOffset Finished { get; set; }
        public TimeSpan Elasped => Finished - Job.Started;
    }
}