using System.Collections.Generic;
using System.IO;

namespace MiniCloud
{
    internal class Job
    {
        public string Arguments { get; set; }
        public string FileName { get; set; }
        public string WorkingDirectory { get; set; }
        public Stream Input { get; set; }
        public IReadOnlyDictionary<string, string> Environment { get; set; }
    }
}