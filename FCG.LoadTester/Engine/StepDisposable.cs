using System;

namespace FCG.LoadTester
{
    class StepDisposable : IDisposable
    {
        private readonly string _name;

        private readonly Api _api;

        public StepDisposable(string name, Api api)
        {
            _name = name;
            _api = api;
        }

        public void Dispose()
        {
            _api.RecordStepEnd(_name);
        }
    }
}