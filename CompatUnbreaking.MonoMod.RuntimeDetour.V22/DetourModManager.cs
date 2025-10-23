using System.Reflection;
using MonoMod.Cil;

namespace MonoMod.RuntimeDetour;

public sealed class DetourModManager : IDisposable
{
    public HashSet<Assembly> Ignored = [];

    public event Action<Assembly, MethodBase, ILContext.Manipulator> OnILHook { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
    public event Action<Assembly, MethodBase, MethodBase, object> OnHook { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
    public event Action<Assembly, MethodBase, MethodBase> OnDetour { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }
    public event Action<Assembly, MethodBase, IntPtr, IntPtr> OnNativeDetour { add => throw new NotImplementedException(); remove => throw new NotImplementedException(); }

    public DetourModManager()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Unload(Assembly asm)
    {
        throw new NotImplementedException();
    }
}
