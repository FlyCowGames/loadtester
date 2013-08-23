using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace FCG.LoadTester
{
    public class LoadTester
    {
        private readonly Action<Api> _action;

        public DateTime? StartTime { get; private set; }

        public DateTime? EndTime { get; private set; }

        public TimeSpan Duration { get; private set; }

        public TestingStatus Status { get; set; }

        public Report Report { get; set; }

        public List<Api> ApiInstances { get; private set; }

        public LoadTester(Action<Api> action)
        {
            _action = action;
            Status = TestingStatus.Idle;
            Report = new Report(this);
            ApiInstances = new List<Api>();
        }

        public Task RunAsync(TimeSpan time, int instanceCount)
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

                ThreadPool.SetMaxThreads(instanceCount, instanceCount);
                ThreadPool.SetMinThreads(instanceCount, instanceCount);

                Enumerable.Range(0, instanceCount).AsParallel().WithDegreeOfParallelism(instanceCount).ForAll(param =>
                {
                    Api api;
                    lock (ApiInstances)
                    {
                        api = new Api(this);
                        ApiInstances.Add(api);
                    }

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
