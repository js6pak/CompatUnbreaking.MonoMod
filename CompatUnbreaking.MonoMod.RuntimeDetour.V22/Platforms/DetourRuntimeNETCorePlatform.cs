using System.Reflection;

namespace MonoMod.RuntimeDetour.Platforms;

public unsafe class DetourRuntimeNETCorePlatform : DetourRuntimeNETPlatform
{
    protected virtual int VTableIndex_ICorJitCompiler_compileMethod { get => throw new NotSupportedAnymoreException(); }
    public override bool OnMethodCompiledWillBeCalled { get => throw new NotSupportedAnymoreException(); }

    public override event OnMethodCompiledEvent OnMethodCompiled { add => throw new NotSupportedAnymoreException(); remove => throw new NotSupportedAnymoreException(); }

    public DetourRuntimeNETCorePlatform()
    {
        throw new NotSupportedAnymoreException();
    }

    protected static IntPtr GetJitObject()
    {
        throw new NotSupportedAnymoreException();
    }

    protected static Guid GetJitGuid(IntPtr jit)
    {
        throw new NotSupportedAnymoreException();
    }

    protected static IntPtr* GetVTableEntry(IntPtr @object, int index)
    {
        throw new NotSupportedAnymoreException();
    }

    protected static IntPtr ReadObjectVTable(IntPtr @object, int index)
    {
        throw new NotSupportedAnymoreException();
    }

    protected override void DisableInlining(MethodBase method, RuntimeMethodHandle handle)
    {
        throw new NotSupportedAnymoreException();
    }

    protected virtual void InstallJitHooks(IntPtr jitObject)
    {
        throw new NotSupportedAnymoreException();
    }

    protected virtual void JitHookCore(RuntimeTypeHandle declaringType, RuntimeMethodHandle methodHandle, IntPtr methodBodyStart, ulong methodBodySize, RuntimeTypeHandle[] genericClassArguments, RuntimeTypeHandle[] genericMethodArguments)
    {
        throw new NotSupportedAnymoreException();
    }

    public static DetourRuntimeNETCorePlatform Create()
    {
        throw new NotSupportedAnymoreException();
    }
}
