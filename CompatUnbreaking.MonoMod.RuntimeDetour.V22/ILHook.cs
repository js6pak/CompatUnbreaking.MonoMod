using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

[UnbreakerReplace(typeof(ILHook))]
public class LegacyILHook : ISortableDetour
{
    private static uint s_globalIndex;

    public static Func<ILHook, MethodBase, ILContext.Manipulator, bool>? OnDetour;
    public static Func<ILHook, bool>? OnUndo;

    public readonly MethodBase Method;
    public readonly ILContext.Manipulator Manipulator;

    public bool IsValid => _hook.IsValid;
    public bool IsApplied => _hook.IsApplied;

    public int Index
    {
        get
        {
            var info = DetourManager.GetDetourInfo(Method);
            using (info.WithLock())
                return info.ILHooks.TakeWhile(d => d != _hook.HookInfo).Count();
        }
    }

    public int MaxIndex
    {
        get
        {
            var info = DetourManager.GetDetourInfo(Method);
            using (info.WithLock())
                return info.Detours.Count();
        }
    }

    public uint GlobalIndex { get; }

    public int Priority
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            Refresh();
        }
    }

    public string ID
    {
        get;
        set
        {
            if (string.IsNullOrEmpty(value))
                value = Manipulator.Method?.GetID(simple: true) ?? GetHashCode().ToString(CultureInfo.InvariantCulture);
            if (field == value)
                return;
            field = value;
            Refresh();
        }
    }

    private readonly List<string> _before = [];
    private ReadOnlyCollection<string>? _beforeReadonly;

    public IEnumerable<string>? Before
    {
        get => _beforeReadonly ??= _before.AsReadOnly();
        set
        {
            lock (_before)
            {
                _before.Clear();
                if (value != null)
                {
                    _before.AddRange(value);
                }

                Refresh();
            }
        }
    }

    private readonly List<string> _after = [];
    private ReadOnlyCollection<string>? _afterReadonly;

    public IEnumerable<string>? After
    {
        get => _afterReadonly ??= _after.AsReadOnly();
        set
        {
            lock (_after)
            {
                _after.Clear();
                if (value != null)
                {
                    _after.AddRange(value);
                }

                Refresh();
            }
        }
    }

    private ILHook _hook;

    public LegacyILHook(MethodBase from, ILContext.Manipulator manipulator, ref ILHookConfig config)
    {
        from = from.GetIdentifiable();
        Method = from;
        Manipulator = manipulator;

        GlobalIndex = Interlocked.Increment(ref s_globalIndex) - 1;

        Priority = config.Priority;
        ID = config.ID;
        if (config.Before != null)
            _before.AddRange(config.Before);
        if (config.After != null)
            _after.AddRange(config.After);

        Create();

        if (!config.ManualApply)
            Apply();
    }

    public LegacyILHook(MethodBase from, ILContext.Manipulator manipulator, ILHookConfig config)
        : this(from, manipulator, ref config)
    {
    }

    public LegacyILHook(MethodBase from, ILContext.Manipulator manipulator)
        : this(from, manipulator, LegacyDetourContext.Current?.ILHookConfig ?? default)
    {
    }

    private void Create()
    {
        _hook = new ILHook(Method, Manipulator, new LegacyDetourConfig
        {
            ID = ID,
            Priority = Priority,
            Before = Before,
            After = After,
        }.ToReorg(GlobalIndex, true), applyByDefault: false);
        GC.SuppressFinalize(_hook);
    }

    private void Refresh()
    {
        if (_hook == null!) return;

        _hook.Dispose();
        Create();
        _hook.Apply();
    }

    public void Apply()
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(ILHook));

        if (IsApplied)
            return;

        if (!(OnDetour?.InvokeWhileTrue(this, Method, Manipulator) ?? true))
            return;

        _hook.Apply();
    }

    public void Undo()
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(ILHook));

        if (!IsApplied)
            return;

        if (!(OnUndo?.InvokeWhileTrue(this) ?? true))
            return;

        _hook.Undo();
    }

    public void Free()
    {
        if (!IsValid)
            return;

        Undo();
        _hook.Dispose();
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
        throw new NotSupportedException();
    }

    public T GenerateTrampoline<T>() where T : Delegate
    {
        throw new NotSupportedException();
    }
}
