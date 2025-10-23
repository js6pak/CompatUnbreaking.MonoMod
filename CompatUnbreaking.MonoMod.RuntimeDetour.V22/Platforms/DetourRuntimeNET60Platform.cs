namespace MonoMod.RuntimeDetour.Platforms;

public unsafe class DetourRuntimeNET60Platform : DetourRuntimeNETCore30Platform
{
    public static readonly Guid JitVersionGuid;

    protected override CorJitResult InvokeRealCompileMethod(IntPtr thisPtr, IntPtr corJitInfo, in CORINFO_METHOD_INFO methodInfo, uint flags, out byte* nativeEntry, out uint nativeSizeOfCode)
    {
        throw new NotSupportedAnymoreException();
    }

    protected override IntPtr GetCompileMethodHook(IntPtr real)
    {
        throw new NotSupportedAnymoreException();
    }
}
