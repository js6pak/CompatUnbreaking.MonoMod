using System.Reflection;

namespace MonoMod.Utils;

public static class GCListener
{
    public static event Action? OnCollect;

    private static bool s_unloading;

    static GCListener()
    {
        _ = new CollectionDummy();

#if NETSTANDARD
        var assemblyLoadContextType = typeof(Assembly).GetTypeInfo().Assembly.GetType("System.Runtime.Loader.AssemblyLoadContext");
        if (assemblyLoadContextType != null)
        {
            var assemblyLoadContext = assemblyLoadContextType.GetMethod("GetLoadContext")!.Invoke(null, [typeof(GCListener).Assembly]);
            var unloadingEvent = assemblyLoadContextType.GetEvent("Unloading")!;
            unloadingEvent.AddEventHandler(assemblyLoadContext, Delegate.CreateDelegate(
                unloadingEvent.EventHandlerType,
                typeof(GCListener).GetMethod("UnloadingALC", BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(assemblyLoadContextType)
            ));
        }
#endif
    }

#if NETSTANDARD
    private static void UnloadingALC<T>(T alc)
    {
        s_unloading = true;
    }
#endif

    private sealed class CollectionDummy
    {
        ~CollectionDummy()
        {
            s_unloading |= AppDomain.CurrentDomain.IsFinalizingForUnload() || Environment.HasShutdownStarted;

            if (!s_unloading)
                GC.ReRegisterForFinalize(this);

            OnCollect?.Invoke();
        }
    }
}
