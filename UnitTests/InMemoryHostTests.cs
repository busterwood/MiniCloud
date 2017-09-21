using MiniCloud;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture, Timeout(1000)]
    public class InMemoryHostTests
    {
        [Test]
        public void can_run()
        {
            var pr = new ProcessRunner();
            var host = new InMemoryHost(new string[0], pr) { Capacity=2 };
            var thread = new Thread(() => host.RunHost());
            thread.Start();
            var job = new Job { WorkingDirectory=@"C:\", FileName="cmd.exe", Arguments="/c dir", Input=new MemoryStream() };
            host.NewJobs.Add(job);
            var result = host.Finished.Take();
            Assert.AreEqual(job, result.Job, "Job");
            Assert.AreEqual(0, result.ExitCode, "ExitCode");
        }
    }
}
