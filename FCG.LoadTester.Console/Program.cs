using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FCG.LoadTester.Console1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var loadTester = new LoadTester(api =>
            {
                var response = api.Get(url: "http://localhost:9593/");
                api.Assert(response.CodeType == ResponseCodes.OK);
            });

            var task = loadTester.RunAsync(time: TimeSpan.FromSeconds(20), instanceCount: 10);

            while (loadTester.Report.Progress < 1)
            {
                Console.WriteLine("Done {0}%", loadTester.Report.Progress * 100);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            Console.WriteLine("Waiting for test to complete.");
            task.Wait();

            Console.WriteLine("Average requests per second: {0}", loadTester.Report.RequestsPerSecond);
        }
    }
}