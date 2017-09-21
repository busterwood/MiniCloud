using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MiniCloud
{
    /// <summary>
    /// Testing host that does not persist the jobs
    /// </summary>
    class InMemoryHost : LoggingHost
    {
        readonly HashSet<Job> running = new HashSet<Job>();

        public BlockingCollection<Job> NewJobs { get; } = new BlockingCollection<Job>();
        public BlockingCollection<JobResult> Finished { get; } = new BlockingCollection<JobResult>();

        public int Capacity { get; set; }

        public InMemoryHost(string[] args, Runner runner) : base (args, runner)
        {
        }

        protected override bool JobCapacityReached()
        {
            lock(running)
            {
                return running.Count >= Capacity;
            }
        }

        protected override async Task StoreExceptionAsync(Job job, Exception ex)
        {
            await base.StoreExceptionAsync(job, ex);
            running.Remove(job);
        }

        protected override async Task StoreResultAsync(Job job, JobResult result)
        {
            await base.StoreResultAsync(job, result);
            result.Finished = DateTimeOffset.Now;
            running.Remove(job);
            Finished.Add(result);
            Console.WriteLine($"{job} exited with code {result.ExitCode}, output is {result.Output.Length} bytes");
        }

        protected override List<Job> WaitForJobs()
        {
            int spareCapacity = SpareCapacity();
            Console.Error.WriteLine($"host has spare capacity of {spareCapacity}");
            if (spareCapacity <= 0)
                return new List<Job>();

            List<Job> result = new List<Job>();
            Job job = NewJobs.Take(); // likely to block here
            job.Started = DateTimeOffset.Now;
            result.Add(job);

            while (result.Count < spareCapacity)
            {
                if (NewJobs.TryTake(out job))
                {
                    job.Started = DateTimeOffset.Now;
                    result.Add(job);
                }
                else
                    break;
            }

            Console.Error.WriteLine($"Returning {result.Count} jobs");
            running.AddRange(result);
            return result;
        }

        private int SpareCapacity()
        {
            lock (running)
            {
                return Math.Max(0, Capacity - running.Count);                
            }
        }
    }
}