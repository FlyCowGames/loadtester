using System;

namespace FCG.LoadTester
{
    public class Report
    {
        private readonly LoadTesterEngine _loadTester;

        public Report(LoadTesterEngine loadTester)
        {
            _loadTester = loadTester;
        }

        public double RequestsPerSecond
        {
            // TODO Very crude implementation. To be improved.
            get
            {
                var timeElapsed = GetTimeElapsed();
                if (timeElapsed == null)
                {
                    return double.NaN;
                }
                var count = _loadTester.EventManager.CountEnd();
                return count / timeElapsed.Value.TotalSeconds;
            }
        }

        public float Progress
        {
            get
            {
                var timeElapsed = GetTimeElapsed();
                var elapsedSeconds = timeElapsed == null ? 0 : timeElapsed.Value.TotalMilliseconds;
                var progress = (float)(elapsedSeconds / _loadTester.Duration.TotalMilliseconds);
                return Math.Min(progress, 1.0F);
            }
        }

        private TimeSpan? GetTimeElapsed()
        {
            var endTime = _loadTester.EndTime ?? DateTime.Now;
            var timeElapsed = endTime - _loadTester.StartTime;
            return timeElapsed;
        }
    }
}