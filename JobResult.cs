﻿using System.IO;

namespace MiniCloud
{
    internal class JobResult
    {
        public Job Job { get; set; }

        public JobResult(Job job)
        {
            this.Job = job;
        }

        public int ExitCode { get; set; }
        public Stream Logging { get; set; }
        public Stream Output { get; set; }
    }
}