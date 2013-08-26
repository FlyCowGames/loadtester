using System.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;
using FCG.LoadTester.Engine;

namespace FCG.LoadTester
{
    public class LoadTesterEngine
    {
        private readonly Action<Api> _action;
        private Action<Api> action;

        public DateTime? StartTime { get; private set; }

        public DateTime? EndTime { get; private set; }

        public TimeSpan Duration { get; private set; }

        public TestingStatus Status { get; set; }

        public Report Report { get; set; }

        public EventManager EventManager { get; set; }

        public LoadTesterEngine(Action<Api> action)
        {
            _action = action;
            Status = TestingStatus.Idle;
            Report = new Report(this);
            EventManager = new EventManager();
        }

        public Task RunAsync(TimeSpan time, int numberOfInstances)
        {
            Duration = time;

            return Task.Factory.StartNew(() =>
            {
                StartTime = DateTime.Now;
                Status = TestingStatus.Running;

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(time);
                    Status = TestingStatus.Stopping;
                });

                ThreadPool.SetMaxThreads(numberOfInstances, numberOfInstances);
                ThreadPool.SetMinThreads(numberOfInstances, numberOfInstances);

                Enumerable.Range(0, numberOfInstances).AsParallel().WithDegreeOfParallelism(numberOfInstances).ForAll(param =>
                {
                    Api api = new Api(this);
                    while (Status == TestingStatus.Running)
                        _action(api);
                });

                Status = TestingStatus.Idle;
                EndTime = DateTime.Now;
            });
        }
    }

    public enum ResponseCodes { OK, Error }
    public enum ResponseDataTypes { HTML, JSON, XML, Undefined }
    public enum RequestCommandType { GET, POST }
    public enum TestingStatus { Idle, Running, Stopping }
}
