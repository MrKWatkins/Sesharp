using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class Event(EventInfo eventInfo) : Member<EventInfo>(eventInfo);