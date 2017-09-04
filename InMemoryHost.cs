using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using System.Text;

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
            running.Remove(job);
            Console.Error.WriteLine($"{job} FAILED with exception {ex}");
            return Task.FromResult(true);
        }

        protected override Task StoreResultAsync(Job job, JobResult result)
        {
            running.Remove(job);
            Console.Error.WriteLine($"{job} exited with code {result.ExitCode}");
            Console.WriteLine($"output is {result.Output.Length} bytes");
            Console.Error.WriteLine(result.Logging.ReadToEnd());
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

    static class IoExtensions
    {
        public static string ReadToEnd(this Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return new StreamReader(stream, Encoding.UTF8, false, 4096, true).ReadToEnd();
        }
    }
}