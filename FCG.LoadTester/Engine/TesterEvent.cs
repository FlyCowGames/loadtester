using System;

namespace FCG.LoadTester
{
    public class TesterEvent
    {
        public DateTime Time { get; set; }
        public string Name { get; set; }
        public TesterEventType Type { get; set; }
    }

    public enum TesterEventType { Start, End, Instant }
}