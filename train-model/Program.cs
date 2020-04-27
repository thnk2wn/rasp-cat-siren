using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CatSiren.TrainModel
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            await new HostBuilder()
                .Create(args)
                .RunConsoleAsync();

            return 0;
        }
    }
}
