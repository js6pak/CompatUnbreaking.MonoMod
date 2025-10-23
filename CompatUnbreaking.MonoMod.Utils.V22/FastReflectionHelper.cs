using System.Reflection;

namespace MonoMod.Utils;

[UnbreakerExtensions]
public static class FastReflectionHelperExtensions
{
    extension(FastReflectionHelper)
    {
        public static FastReflectionDelegate CreateFastDelegate([UnbreakerThis] MethodInfo method, bool directBoxValueAccess = true)
        {
            return GetFastDelegate(method, directBoxValueAccess);
        }

        public static FastReflectionDelegate GetFastDelegate([UnbreakerThis] MethodInfo method, bool directBoxValueAccess = true)
        {
            if (!directBoxValueAccess) throw new NotImplementedException();
            return new FastReflectionDelegate(method.GetFastInvoker());
        }
    }
}
