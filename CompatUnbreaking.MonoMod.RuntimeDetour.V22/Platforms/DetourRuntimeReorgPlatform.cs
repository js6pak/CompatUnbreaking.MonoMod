using System.Reflection;
using MonoMod.Core.Platforms;

namespace MonoMod.RuntimeDetour.Platforms;

internal sealed class DetourRuntimeReorgPlatform : DetourRuntimeILPlatform
{
    public static DetourRuntimeReorgPlatform Instance { get; } = new();

    public override bool OnMethodCompiledWillBeCalled => PlatformTriple.Current.SupportedFeatures.Has(RuntimeFeature.CompileMethodHook);
    public override event OnMethodCompiledEvent? OnMethodCompiled;

    private DetourRuntimeReorgPlatform()
    {
        PlatformTriple.Current.Runtime.OnMethodCompiled += (handle, method, start, rw, size) =>
        {
            OnMethodCompiled?.Invoke(method, start, size);
        };
    }

    protected override RuntimeMethodHandle GetMethodHandle(MethodBase method)
    {
        return PlatformTriple.Current.Runtime.GetMethodHandle(method);
    }
}
