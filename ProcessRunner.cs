using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace MiniCloud
{
    class ProcessRunner : Runner
    {
        public override async Task<JobResult> RunAsync(Job job)
        {
            var result = new JobResult(job);

            ProcessStartInfo start = new ProcessStartInfo
            {
                Arguments = job.Arguments,
                WorkingDirectory = job.WorkingDirectory,
                FileName = job.FileName,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            start.EnvironmentVariables.AddRange(job.Environment);

            var proc = Process.Start(start);

            var loggingTask = proc.StandardError.BaseStream.CopyToAsync(result.Logging);
            var outputTask = proc.StandardOutput.BaseStream.CopyToAsync(result.Output);
            var inputTask = job.Input.CopyToAsync(proc.StandardInput.BaseStream); // TODO:: wait?
            proc.WaitForExit();

            result.ExitCode = proc.ExitCode;
            
            // make sure all output has been copied
            await loggingTask; 
            await outputTask;
            return result;
        }
    }

    static class Extensions
    {
        public static void AddRange(this StringDictionary dict, IEnumerable<KeyValuePair<string, string>> values)
        {
            foreach (var pair in values)
            {
                dict.Add(pair.Key, pair.Value);
            }
        }
    }
}