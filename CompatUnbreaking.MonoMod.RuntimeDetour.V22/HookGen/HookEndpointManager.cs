using System.Reflection;

namespace MonoMod.RuntimeDetour.HookGen;

[UnbreakerExtensions]
public static class HookEndpointManagerExtensions
{
    extension(HookEndpointManager)
    {
        public static object GetOwner(Delegate hook)
        {
            throw new NotImplementedException();
        }

        public static void RemoveAllOwnedBy(object owner)
        {
            throw new NotImplementedException();
        }
    }

    [UnbreakerExtension(typeof(HookEndpointManager))]
    public static class HookEndpointManagerExtension
    {
        public static event Func<Delegate, object> OnGetOwner { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public static event Func<object, bool> OnRemoveAllOwnedBy { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public static event Func<MethodBase, Delegate, bool> OnAdd { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public static event Func<MethodBase, Delegate, bool> OnRemove { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public static event Func<MethodBase, Delegate, bool> OnModify { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
        public static event Func<MethodBase, Delegate, bool> OnUnmodify { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
    }
}
