using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FCG.LoadTester.Engine
{
    public class EventManager
    {
        private readonly object _syncLock;

        private readonly ConcurrentDictionary<object, List<TesterEvent>> _eventListTable;

        public EventManager()
        {
            _syncLock = new object();
            _eventListTable = new ConcurrentDictionary<object, List<TesterEvent>>();
        }

        public void Add(object key, TesterEvent testerEvent)
        {
            lock (_syncLock)
            {
                List<TesterEvent> eventList;
                if (!_eventListTable.TryGetValue(key, out eventList))
                {
                    eventList = new List<TesterEvent>();
                    _eventListTable[key] = eventList;
                }
                eventList.Add(testerEvent);
            }
        }

        public int CountEnd()
        {
            lock (_syncLock)
            {
                return _eventListTable.Values.Sum(eventList => eventList.Count(e => e.Type == TesterEventType.End));
            }
        }
    }
}
