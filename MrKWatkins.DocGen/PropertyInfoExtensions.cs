using System.Reflection;
using System.Runtime.CompilerServices;

namespace MrKWatkins.DocGen;

// TODO: GetBaseDefinition, GetVirtuality, IsNew
public static class PropertyInfoExtensions
{
    [Pure]
    public static bool HasInitSetter(this PropertyInfo propertyInfo) =>
        propertyInfo.SetMethod?.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)) == true;

    [Pure]
    public static bool HasPublicOrProtectedOverloads(this PropertyInfo property)
    {
        if (!property.IsIndexer())
        {
            return false;
        }

        var type = property.DeclaringType!;
        var indexers = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(p => p != property && p.IsIndexer() && (PropertyAccessorIsPublicOrProtected(p.GetMethod) || PropertyAccessorIsPublicOrProtected(p.SetMethod)));
        return indexers.Any();
    }

    [Pure]
    public static bool IsAbstract(this PropertyInfo property) => property.GetMethod?.IsAbstract == true || property.SetMethod?.IsAbstract == true;

    [Pure]
    public static bool IsAbstractOrVirtual(this PropertyInfo property) => property.GetMethod?.IsVirtual == true || property.SetMethod?.IsVirtual == true;

    [Pure]
    public static bool IsIndexer(this PropertyInfo propertyInfo) => propertyInfo.GetIndexParameters().Length > 0;

    [Pure]
    public static bool IsProtected(this PropertyInfo property)
    {
        var getMethod = property.GetMethod;
        var setMethod = property.SetMethod;
        if (getMethod == null)
        {
            return setMethod!.IsProtected();
        }

        if (setMethod == null)
        {
            return getMethod.IsProtected();
        }

        if (getMethod.IsPublic || setMethod.IsPublic)
        {
            return false;
        }

        return getMethod.IsProtected() || setMethod.IsProtected();
    }

    [Pure]
    public static bool IsPublic(this PropertyInfo property) => property.GetMethod?.IsPublic == true || property.SetMethod?.IsPublic == true;

    [Pure]
    public static bool IsPublicOrProtected(this PropertyInfo property) =>
        property.GetMethod?.IsPublicOrProtected() == true || property.SetMethod?.IsPublicOrProtected() == true;

    [Pure]
    public static bool IsRequired(this PropertyInfo propertyInfo) => propertyInfo.IsDefined(typeof(RequiredMemberAttribute));

    [Pure]
    private static bool PropertyAccessorIsPublicOrProtected(MethodInfo? accessor) => accessor == null || accessor.IsPublicOrProtected();
}