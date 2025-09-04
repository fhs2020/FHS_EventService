using FHS_EventService.Models;

namespace FHS_EventService.Storage
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly Queue<UserEvent> _queue = new();
        private readonly int _max;

        public InMemoryEventStore(int max)
        {
            _max = max;
        }

        public int Count => _queue.Count;

        public void Append(UserEvent ev)
        {
            _queue.Enqueue(ev);

            while (_queue.Count > _max)
            {
                _queue.Dequeue();
            }
        }

        public IReadOnlyList<UserEvent> GetLatest(int limit)
        {
            if (limit < 1) limit = 1;
            if (limit > _max) limit = _max;

            var list = _queue.ToList();
            int start = Math.Max(0, list.Count - limit);
            return list.Skip(start).ToList();
        }
    }
}
