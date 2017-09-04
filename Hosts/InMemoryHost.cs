using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MiniCloud
{
    /// <summary>
    /// Testing host that does not persist the jobs
    /// </summary>
    class InMemoryHost : Host
    {
        readonly HashSet<Job> running = new HashSet<Job>();

        public BlockingCollection<Job> NewJobs { get; } = new BlockingCollection<Job>();

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

        protected override Task StoreExceptionAsync(Job job, Exception ex)
        {
            //result.Finished = DateTimeOffset.Now;
            running.Remove(job);
            Console.Error.WriteLine($"{job} FAILED with exception {ex}");
            return Task.FromResult(true);
        }

        protected override Task StoreResultAsync(Job job, JobResult result)
        {
            result.Finished = DateTimeOffset.Now;
            running.Remove(job);
            Console.WriteLine($"{job} exited with code {result.ExitCode} in {result.Elasped.TotalMilliseconds:N0}MS, output is {result.Output.Length} bytes");
            Console.Error.WriteLine(result.Logging.ReadToEnd());
            return Task.FromResult(true);
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