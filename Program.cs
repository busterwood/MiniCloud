using System.IO;

namespace MiniCloud
{

    class Program
    {
        public static void Main(string[] args)
        {
            var p = new InMemoryHost(args, new ProcessRunner());
            p.Capacity = 2;
            p.NewJobs.Add(new Job { WorkingDirectory = "c:\\windows\\system32", FileName = "cmd.exe", Arguments = "/c dir", Input = new MemoryStream() });
            p.NewJobs.Add(new Job { WorkingDirectory = "c:\\windows\\system", FileName = "cmd.exe", Arguments = "/c dir", Input = new MemoryStream() });
            p.NewJobs.Add(new Job { WorkingDirectory = "c:\\windows", FileName = "cmd.exe", Arguments = "/c dir", Input = new MemoryStream() });
            p.NewJobs.Add(new Job { WorkingDirectory = "c:\\", FileName = "cmd.exe", Arguments = "/c dir", Input = new MemoryStream() });
            p.NewJobs.Add(new Job { WorkingDirectory = ".", FileName = "cmd.exe", Arguments = "/c dir", Input = new MemoryStream() });
            p.RunHost();
        }        
    }
}
