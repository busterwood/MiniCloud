using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MiniCloud
{
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
            running.Remove(job);
            return Task.FromResult(true);
        }

        protected override Task StoreResultAsync(Job job, JobResult result)
        {
            running.Remove(job);
            return Task.FromResult(true);
        }

        protected override List<Job> WaitForJobs()
        {
            int spareCapacity = SpareCapacity();
            if (spareCapacity <= 0)
                return new List<Job>();

            List<Job> result = new List<Job>();
            result.Add(NewJobs.Take()); // likely to block here

            while (result.Count < spareCapacity)
            {
                Job job;
                if (NewJobs.TryTake(out job))
                    result.Add(job);
                else
                    break;
            }

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