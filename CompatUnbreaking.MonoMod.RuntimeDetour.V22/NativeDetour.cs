using System.Reflection;
using MonoMod.Core;
using MonoMod.Core.Platforms;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public class NativeDetour : IDetour
{
    public static Func<NativeDetour, MethodBase?, IntPtr, IntPtr, bool>? OnDetour;
    public static Func<NativeDetour, bool>? OnUndo;
    public static Func<NativeDetour, MethodBase?, MethodBase>? OnGenerateTrampoline;

    private readonly HashSet<MethodBase> _pinned = [];
    private ICoreNativeDetour _detour;
    private readonly MethodInfo? _backupMethod;

    public bool IsValid { get; private set; }
    public bool IsApplied => _detour.IsApplied;

    public NativeDetourData Data { get; private set; }
    public readonly MethodBase? Method;


    public NativeDetour(MethodBase? method, IntPtr from, IntPtr to, ref NativeDetourConfig config)
    {
        if (from == to)
            throw new InvalidOperationException($"Cannot detour from a location to itself! (from: {from:X16} to: {to:X16} method: {method})");

        method = method?.GetIdentifiable();
        Method = method;

        if (!(OnDetour?.InvokeWhileTrue(this, method, from, to) ?? true))
            return;
        IsValid = true;

        Create(from, to);

        if (!config.SkipILCopy)
            method?.TryCreateILCopy(out _backupMethod);

        if (!config.ManualApply)
            Apply();
    }

    private void Create(IntPtr from, IntPtr to)
    {
        _detour = DetourFactory.Current.CreateNativeDetour(from, to, false);
        GC.SuppressFinalize(_detour);

        Data = new NativeDetourData
        {
            Method = _detour.Source,
            Target = _detour.Target,
        };
    }

    public NativeDetour(MethodBase? method, IntPtr from, IntPtr to, NativeDetourConfig config)
        : this(method, from, to, ref config)
    {
    }

    public NativeDetour(MethodBase? method, IntPtr from, IntPtr to)
        : this(method, from, to, default)
    {
    }

    public NativeDetour(IntPtr from, IntPtr to, ref NativeDetourConfig config)
        : this(null, from, to, ref config)
    {
    }

    public NativeDetour(IntPtr from, IntPtr to, NativeDetourConfig config)
        : this(null, from, to, ref config)
    {
    }

    public NativeDetour(IntPtr from, IntPtr to)
        : this(null, from, to)
    {
    }

    public NativeDetour(MethodBase from, IntPtr to, ref NativeDetourConfig config)
        : this(from, from.Pin().GetNativeStart(), to, ref config)
    {
        _pinned.Add(from);
    }

    public NativeDetour(MethodBase from, IntPtr to, NativeDetourConfig config)
        : this(from, from.Pin().GetNativeStart(), to, ref config)
    {
        _pinned.Add(from);
    }

    public NativeDetour(MethodBase from, IntPtr to)
        : this(from, from.Pin().GetNativeStart(), to)
    {
        _pinned.Add(from);
    }

    public NativeDetour(IntPtr from, MethodBase to, ref NativeDetourConfig config)
        : this(from, to.Pin().GetNativeStart(), ref config)
    {
        _pinned.Add(to);
    }

    public NativeDetour(IntPtr from, MethodBase to, NativeDetourConfig config)
        : this(from, to.Pin().GetNativeStart(), ref config)
    {
        _pinned.Add(to);
    }

    public NativeDetour(IntPtr from, MethodBase to)
        : this(from, to.Pin().GetNativeStart())
    {
        _pinned.Add(to);
    }

    public NativeDetour(MethodBase from, MethodBase to, ref NativeDetourConfig config)
        : this(from.Pin().GetNativeStart(), DetourHelper.Runtime.GetDetourTarget(from, to), ref config)
    {
        _pinned.Add(from);
    }

    public NativeDetour(MethodBase from, MethodBase to, NativeDetourConfig config)
        : this(from.Pin().GetNativeStart(), DetourHelper.Runtime.GetDetourTarget(from, to), ref config)
    {
        _pinned.Add(from);
    }

    public NativeDetour(MethodBase from, MethodBase to)
        : this(from.Pin().GetNativeStart(), DetourHelper.Runtime.GetDetourTarget(from, to))
    {
        _pinned.Add(from);
    }

    public NativeDetour(Delegate from, IntPtr to, ref NativeDetourConfig config)
        : this(from.Method, to, ref config)
    {
    }

    public NativeDetour(Delegate from, IntPtr to, NativeDetourConfig config)
        : this(from.Method, to, ref config)
    {
    }

    public NativeDetour(Delegate from, IntPtr to)
        : this(from.Method, to)
    {
    }

    public NativeDetour(IntPtr from, Delegate to, ref NativeDetourConfig config)
        : this(from, to.Method, ref config)
    {
    }

    public NativeDetour(IntPtr from, Delegate to, NativeDetourConfig config)
        : this(from, to.Method, ref config)
    {
    }

    public NativeDetour(IntPtr from, Delegate to)
        : this(from, to.Method)
    {
    }

    public NativeDetour(Delegate from, Delegate to, ref NativeDetourConfig config)
        : this(from.Method, to.Method, ref config)
    {
    }

    public NativeDetour(Delegate from, Delegate to, NativeDetourConfig config)
        : this(from.Method, to.Method, ref config)
    {
    }

    public NativeDetour(Delegate from, Delegate to)
        : this(from.Method, to.Method)
    {
    }

    public void Apply()
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(NativeDetour));

        if (IsApplied)
            return;

        _detour.Apply();
    }

    public void Undo()
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(NativeDetour));

        if (!(OnUndo?.InvokeWhileTrue(this) ?? true))
            return;

        if (!IsApplied)
            return;

        _detour.Undo();
    }

    public void ChangeSource(IntPtr newSource)
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(NativeDetour));

        Create(newSource, _detour.Target);
        Apply();
    }

    public void ChangeTarget(IntPtr newTarget)
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(NativeDetour));

        Create(_detour.Source, newTarget);
        Apply();
    }

    public void Free()
    {
        if (!IsValid)
            return;
        IsValid = false;

        if (!IsApplied)
        {
            foreach (var method in _pinned)
                method.Unpin();
            _pinned.Clear();
        }
    }

    public void Dispose()
    {
        if (!IsValid)
            return;

        Undo();
        Free();
    }

    public MethodBase GenerateTrampoline(MethodBase? signature = null)
    {
        var remoteTrampoline = OnGenerateTrampoline?.InvokeWhileNull<MethodBase>(this, signature);
        if (remoteTrampoline != null)
            return remoteTrampoline;

        if (!IsValid)
            throw new ObjectDisposedException(nameof(NativeDetour));

        if (_backupMethod != null)
        {
            return _backupMethod;
        }

        if (signature == null)
            throw new ArgumentNullException(nameof(signature), "A signature must be given if the NativeDetour doesn't hold a reference to a managed method.");

        if (!_detour.HasOrigEntrypoint)
            throw new InvalidOperationException();

        return DetourHelper.GenerateNativeProxy(_detour.OrigEntrypoint, signature);
    }

    public T GenerateTrampoline<T>() where T : Delegate
    {
        if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
            throw new InvalidOperationException($"Type {typeof(T)} not a delegate type.");

        return GenerateTrampoline(typeof(T).GetMethod("Invoke")).CreateDelegate<T>();
    }
}
