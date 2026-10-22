using System.Reflection;

namespace MrKWatkins.Sesharp.Model;

public sealed class PropertyGroup(string name, [InstantHandle] IEnumerable<Property> properties) : MemberGroup<Property, PropertyInfo>(name, properties)
{
    public override string GroupFileName => Name;
}