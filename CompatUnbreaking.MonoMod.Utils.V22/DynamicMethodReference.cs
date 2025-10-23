using System.Reflection;

namespace MonoMod.Utils;

[UnbreakerExtensions]
public static class DynamicMethodReferenceExtensions
{
    extension(DynamicMethodReference @this)
    {
        [UnbreakerField]
        public MethodInfo DynamicMethod => @this.DynamicMethod;
    }
}
