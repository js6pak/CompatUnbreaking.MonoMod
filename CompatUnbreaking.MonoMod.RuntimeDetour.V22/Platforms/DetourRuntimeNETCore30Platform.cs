using System.Reflection;

namespace MonoMod.RuntimeDetour.Platforms;

public unsafe class DetourRuntimeNETCore30Platform : DetourRuntimeNETCorePlatform
{
    public static readonly Guid JitVersionGuid;
    protected d_MethodHandle_GetLoaderAllocator MethodHandle_GetLoaderAllocator;
    protected d_CreateRuntimeMethodInfoStub CreateRuntimeMethodInfoStub;
    protected d_CreateRuntimeMethodHandle CreateRuntimeMethodHandle;
    protected d_GetDeclaringTypeOfMethodHandle GetDeclaringTypeOfMethodHandle;
    protected d_GetTypeFromNativeHandle GetTypeFromNativeHandle;

    public override bool OnMethodCompiledWillBeCalled { get => throw new NotSupportedAnymoreException(); }

    protected override void DisableInlining(MethodBase method, RuntimeMethodHandle handle)
    {
        throw new NotSupportedAnymoreException();
    }

    protected virtual CorJitResult InvokeRealCompileMethod(IntPtr thisPtr, IntPtr corJitInfo, in CORINFO_METHOD_INFO methodInfo, uint flags, out byte* nativeEntry, out uint nativeSizeOfCode)
    {
        throw new NotSupportedAnymoreException();
    }

    protected virtual IntPtr GetCompileMethodHook(IntPtr real)
    {
        throw new NotSupportedAnymoreException();
    }

    protected override void InstallJitHooks(IntPtr jit)
    {
        throw new NotSupportedAnymoreException();
    }

    protected static NativeDetourData CreateNativeTrampolineTo(IntPtr target)
    {
        throw new NotSupportedAnymoreException();
    }

    protected static void FreeNativeTrampoline(NativeDetourData data)
    {
        throw new NotSupportedAnymoreException();
    }

    protected CorJitResult CompileMethodHook(IntPtr jit, IntPtr corJitInfo, in CORINFO_METHOD_INFO methodInfo, uint flags, out byte* nativeEntry, out uint nativeSizeOfCode)
    {
        throw new NotSupportedAnymoreException();
    }

    protected RuntimeMethodHandle CreateHandleForHandlePointer(IntPtr handle)
    {
        throw new NotSupportedAnymoreException();
    }

    protected virtual void SetupJitHookHelpers()
    {
        throw new NotSupportedAnymoreException();
    }

    protected virtual void MakeAssemblySystemAssembly(Assembly assembly)
    {
        throw new NotSupportedAnymoreException();
    }

    protected void HookPermanent(MethodBase from, MethodBase to)
    {
        throw new NotSupportedAnymoreException();
    }

    protected void HookPermanent(IntPtr from, IntPtr to)
    {
        throw new NotSupportedAnymoreException();
    }

    protected enum CorJitResult
    {
        CORJIT_OK = 0,
    }

    protected struct CORINFO_SIG_INST
    {
        public uint classInstCount;
        public IntPtr* classInst;
        public uint methInstCount;
        public IntPtr* methInst;
    }

    protected struct CORINFO_SIG_INFO
    {
        public int callConv;
        public IntPtr retTypeClass;
        public IntPtr retTypeSigClass;
        public byte retType;
        public byte flags;
        public ushort numArgs;
        public CORINFO_SIG_INST sigInst;
        public IntPtr args;
        public IntPtr pSig;
        public uint sbSig;
        public IntPtr scope;
        public uint token;
    }

    protected struct CORINFO_METHOD_INFO
    {
        public IntPtr ftn;
        public IntPtr scope;
        public byte* ILCode;
        public uint ILCodeSize;
        public uint maxStack;
        public uint EHcount;
        public int options;
        public int regionKind;
        public CORINFO_SIG_INFO args;
        public CORINFO_SIG_INFO locals;
    }

    protected delegate object d_MethodHandle_GetLoaderAllocator(IntPtr methodHandle);

    protected delegate object d_CreateRuntimeMethodInfoStub(IntPtr methodHandle, object loaderAllocator);

    protected delegate RuntimeMethodHandle d_CreateRuntimeMethodHandle(object runtimeMethodInfo);

    protected delegate Type d_GetDeclaringTypeOfMethodHandle(IntPtr methodHandle);

    protected delegate Type d_GetTypeFromNativeHandle(IntPtr handle);
}
