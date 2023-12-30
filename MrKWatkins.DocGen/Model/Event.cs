using System.Reflection;

namespace MrKWatkins.DocGen.Model;

public sealed class Event : Member<EventInfo>
{
    public Event(EventInfo eventInfo)
        : base(eventInfo)
    {
    }
}