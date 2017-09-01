namespace MiniCloud
{

    class Program
    {
        public void Main(string[] args)
        {
            var p = new InMemoryHost(args, new ProcessRunner());
            p.RunHost();
        }        
    }
}
