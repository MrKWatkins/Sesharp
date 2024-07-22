using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Event : Member<EventInfo>
{
    public Event(EventInfo eventInfo)
        : base(eventInfo)
    {
    }
}