using System.Threading.Tasks;

namespace MiniCloud
{
    abstract class Runner
    {
        public abstract Task<JobResult> RunAsync(Job job);
    }
}