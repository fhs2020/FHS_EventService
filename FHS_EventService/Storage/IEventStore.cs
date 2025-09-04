using FHS_EventService.Models;

namespace FHS_EventService.Storage
{
    public interface IEventStore
    {
        void Append(UserEvent ev);
        IReadOnlyList<UserEvent> GetLatest(int limit);
        int Count { get; }
    }
}
