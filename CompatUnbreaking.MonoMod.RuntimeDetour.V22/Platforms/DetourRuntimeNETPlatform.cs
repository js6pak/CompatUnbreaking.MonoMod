using System.Reflection;

namespace MonoMod.RuntimeDetour.Platforms;

public class DetourRuntimeNETPlatform : DetourRuntimeILPlatform
{
    public override bool OnMethodCompiledWillBeCalled { get => throw new NotSupportedAnymoreException(); }

    public override event OnMethodCompiledEvent OnMethodCompiled { add => throw new NotSupportedAnymoreException(); remove => throw new NotSupportedAnymoreException(); }

    public override MethodBase GetIdentifiable(MethodBase method)
    {
        throw new NotSupportedAnymoreException();
    }

    protected override RuntimeMethodHandle GetMethodHandle(MethodBase method)
    {
        throw new NotSupportedAnymoreException();
    }

    protected override void DisableInlining(MethodBase method, RuntimeMethodHandle handle)
    {
        throw new NotSupportedAnymoreException();
    }

    protected override IntPtr GetFunctionPointer(MethodBase method, RuntimeMethodHandle handle)
    {
        throw new NotSupportedAnymoreException();
    }
}
