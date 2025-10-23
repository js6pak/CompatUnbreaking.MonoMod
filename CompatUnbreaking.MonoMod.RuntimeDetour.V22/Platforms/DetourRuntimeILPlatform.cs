using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using MonoMod.Core.Platforms;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour.Platforms;

public abstract class DetourRuntimeILPlatform : IDetourRuntimePlatform
{
    protected GlueThiscallStructRetPtrOrder GlueThiscallStructRetPtr;
    protected GlueThiscallStructRetPtrOrder GlueThiscallInStructRetPtr;
    protected ConcurrentDictionary<MethodBase, PrivateMethodPin> PinnedMethods;
    protected ConcurrentDictionary<RuntimeMethodHandle, PrivateMethodPin> PinnedHandles;

    public abstract bool OnMethodCompiledWillBeCalled { get; }

    public abstract event OnMethodCompiledEvent OnMethodCompiled;

    public DetourRuntimeILPlatform()
    {
    }

    protected abstract RuntimeMethodHandle GetMethodHandle(MethodBase method);

    protected virtual IntPtr GetFunctionPointer(MethodBase method, RuntimeMethodHandle handle)
    {
        return PlatformTriple.Current.Runtime.GetMethodEntryPoint(method);
    }

    protected virtual void PrepareMethod(MethodBase method, RuntimeMethodHandle handle)
    {
        PlatformTriple.Current.Compile(method);
    }

    protected virtual void PrepareMethod(MethodBase method, RuntimeMethodHandle handle, RuntimeTypeHandle[] instantiation)
    {
        RuntimeHelpers.PrepareMethod(handle, instantiation);
    }

    protected virtual void DisableInlining(MethodBase method, RuntimeMethodHandle handle)
    {
        PlatformTriple.Current.TryDisableInlining(method);
    }

    public virtual MethodBase GetIdentifiable(MethodBase method)
    {
        return PlatformTriple.Current.GetIdentifiable(method);
    }

    public virtual MethodPinInfo GetPin(MethodBase method)
    {
        throw new NotSupportedAnymoreException();
    }

    public virtual MethodPinInfo GetPin(RuntimeMethodHandle handle)
    {
        throw new NotSupportedAnymoreException();
    }

    public virtual MethodPinInfo[] GetPins()
    {
        throw new NotSupportedAnymoreException();
    }

    public virtual IntPtr GetNativeStart(MethodBase method)
    {
        return PlatformTriple.Current.GetNativeMethodBody(method);
    }

    private readonly ConcurrentDictionary<MethodBase, IDisposable> _pinnedMethods = new();

    public virtual void Pin(MethodBase method)
    {
        method = GetIdentifiable(method);

        var pin = PlatformTriple.Current.PinMethodIfNeeded(method);
        if (pin != null)
        {
            _pinnedMethods.TryAdd(method, pin);
        }
    }

    public virtual void Unpin(MethodBase method)
    {
        if (_pinnedMethods.TryGetValue(method.GetIdentifiable(), out var pin))
        {
            pin.Dispose();
        }
    }

    public MethodInfo CreateCopy(MethodBase method)
    {
        using var dmd = new DynamicMethodDefinition(method);
        return dmd.Generate();
    }

    public bool TryCreateCopy(MethodBase method, out MethodInfo? dm)
    {
        try
        {
            dm = CreateCopy(method);
            return true;
        }
        catch
        {
            dm = null;
            return false;
        }
    }

    public MethodBase GetDetourTarget(MethodBase from, MethodBase to)
    {
        return PlatformTriple.Current.GetRealDetourTarget(from, to);
    }

    public uint TryMemAllocScratchCloseTo(IntPtr target, out IntPtr ptr, int size)
    {
        if (size == -1) size = (int) _MemAllocScratchDummySafeSize;

        var allocationRequest = new PositionedAllocationRequest(
            target,
            target + int.MinValue,
            target + int.MaxValue,
            new AllocationRequest(size) { Executable = true }
        );

        if (PlatformTriple.Current.System.MemoryAllocator.TryAllocateInRange(allocationRequest, out var allocated))
        {
            ptr = allocated.BaseAddress;
            return (uint) allocated.Size;
        }

        ptr = IntPtr.Zero;
        return 0;
    }

    protected static readonly uint _MemAllocScratchDummySafeSize = 16;

    protected static readonly MethodInfo _MemAllocScratchDummy =
        typeof(DetourRuntimeILPlatform).GetMethod(nameof(MemAllocScratchDummy), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

    public static int MemAllocScratchDummy(int a, int b)
    {
        if (a >= 1024 && b >= 1024)
            return a + b;
        return MemAllocScratchDummy(a + b, b + 1);
    }

    protected class PrivateMethodPin
    {
        public MethodPinInfo Pin;
    }

    public struct MethodPinInfo
    {
        public int Count;
        public MethodBase Method;
        public RuntimeMethodHandle Handle;

        public override string ToString()
        {
            return $"(MethodPinInfo: {Count}, {Method}, 0x{(long) Handle.Value:X})";
        }
    }

    protected enum GlueThiscallStructRetPtrOrder
    {
        Original = 0,
        ThisRetArgs = 1,
        RetThisArgs = 2,
    }
}
