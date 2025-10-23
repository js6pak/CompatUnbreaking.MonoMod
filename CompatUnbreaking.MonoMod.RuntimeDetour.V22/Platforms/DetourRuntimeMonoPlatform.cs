using System.Reflection;

namespace MonoMod.RuntimeDetour.Platforms;

public class DetourRuntimeMonoPlatform : DetourRuntimeILPlatform
{
    public override bool OnMethodCompiledWillBeCalled { get => throw new NotSupportedAnymoreException(); }

    public override event OnMethodCompiledEvent OnMethodCompiled { add => throw new NotSupportedAnymoreException(); remove => throw new NotSupportedAnymoreException(); }

    protected override RuntimeMethodHandle GetMethodHandle(MethodBase method)
    {
        throw new NotSupportedAnymoreException();
    }

    protected override void DisableInlining(MethodBase method, RuntimeMethodHandle handle)
    {
        throw new NotSupportedAnymoreException();
    }
}
