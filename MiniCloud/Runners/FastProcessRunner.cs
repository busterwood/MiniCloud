using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.IO;
using System.Text;

namespace MiniCloud
{
    /// <summary>
    /// Runs a job in a separate process but reuses the process using the following protocol:
    /// 
    /// StdIn is sent a GUID on a new line, then the request followed by the GUID " end" on a new line.
    /// StdErr responses are read in the same way: GUID on a new line, then the result followed by the GUID " end" on a new line.
    /// StdOut responses are similar: GUID on a new line, then the result followed by the GUID " end" on a new line, followed by the exit code on the next new line.
    /// </summary>
    class FastProcessRunner : Runner
    {
        public override async Task<JobResult> RunAsync(Job job)
        {
            await Task.Yield(); // ensure not running synchronously

            var proc = GetOrCreateProcess(job);
            Console.Error.WriteLine($"{job} starting");

            var result = new JobResult(job);
            var loggingTask = CopyLoggingAsync(job.Id, proc.StandardError, result.Logging);
            var outputTask = CopyOutputAsync(job.Id, proc.StandardOutput, result.Output, result);
            var inputTask = CopyInputAsync(job.Id, job.Input, proc.StandardInput);
            // make sure all output has been copied
            await loggingTask;
            await outputTask;
            return result;
        }

        async Task CopyLoggingAsync(Guid id, StreamReader input, Stream output)
        {
            var guid = id.ToString("D");
            await ReadStartGuid(guid, input, "StdErr");
            await CopyUntilEndAsync(guid + " end", input, output, "StdErr");
        }

        async Task ReadStartGuid(string start, TextReader input, string pipeName)
        {
            for (;;)
            {
                var line = await input.ReadLineAsync();
                if (line == null)
                    throw new Exception($"Unexpected end of {pipeName} for job {start}");
                if (line != start)
                    break;
                // skip line, log this?
                Log.Debug("Skipped line at start of request", new { JobId=start, Pipe = pipeName, line });
            }
        }

        async Task CopyUntilEndAsync(string end, TextReader input, Stream output, string pipeName)
        {
            // read request until end
            var outputWriter = new StreamWriter(output, Encoding.UTF8, 1024, true);
            for (;;)
            {
                var line = await input.ReadLineAsync();
                if (line == null)
                    throw new Exception($"Unexpected end of {pipeName} for job {end}");
                if (line == end)
                    break;
                await outputWriter.WriteLineAsync(line);
            }
            await outputWriter.FlushAsync();
        }

        async Task CopyOutputAsync(Guid id, StreamReader input, Stream output, JobResult result)
        {
            var guid = id.ToString("D");
            await ReadStartGuid(guid, input, "StdErr");
            await CopyUntilEndAsync(guid + " end", input, output, "StdErr");
            await ReadExitCode(id, input, result);
        }

        private static async Task ReadExitCode(Guid id, StreamReader input, JobResult result)
        {
            var exitCodeText = await input.ReadLineAsync();
            int ec;
            if (int.TryParse(exitCodeText, out ec))
                result.ExitCode = ec;
            else
                throw new Exception($"Unexpected exit code for job {id}: '{exitCodeText}'");
        }

        async Task CopyInputAsync(Guid id, Stream input, TextWriter output)
        {
            var guid = id.ToString("D");
            var inputReader = new StreamReader(input, Encoding.UTF8, false, 1024, true);
            await output.WriteLineAsync(guid);
            for(;;)
            {
                var line = await inputReader.ReadLineAsync();
                if (line == null)
                    break;
            }
            await output.WriteLineAsync(guid + " end");
        }

        Process GetOrCreateProcess(Job job)
        {
            //TODO: cache the process based on filename, working folder, env and args

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

            Console.Error.WriteLine($"Starting FastProcess for {job}");
            return Process.Start(start);
        }
    }
}
