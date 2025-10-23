using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public class Detour : ISortableDetour
{
    private static uint s_globalIndex;

    public static Func<Detour, MethodBase, MethodBase, bool>? OnDetour;
    public static Func<Detour, bool>? OnUndo;
    public static Func<Detour, MethodBase?, MethodBase>? OnGenerateTrampoline;

    public readonly MethodBase Method;
    public readonly MethodBase Target;
    public readonly MethodBase TargetReal;
    private readonly object? TargetObj;

    public bool IsValid => _hook.IsValid;
    public bool IsApplied => _hook.IsApplied;

    public int Index
    {
        get
        {
            var info = DetourManager.GetDetourInfo(Method);
            using (info.WithLock())
                return info.Detours.TakeWhile(d => d != _hook.DetourInfo).Count();
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
                value = Target.GetID(simple: true);
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

    private Hook _hook;

    internal Detour(MethodBase from, MethodBase to, object? targetObj, in LegacyDetourConfig config)
    {
        from = from.GetIdentifiable();
        if (from.Equals(to))
            throw new ArgumentException("Cannot detour a method to itself!");

        Method = from;
        Target = to;
        TargetReal = DetourHelper.Runtime.GetDetourTarget(from, to);
        TargetObj = targetObj;

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

    private void Create()
    {
        _hook = new Hook(Method, (MethodInfo) Target, TargetObj, new LegacyDetourConfig
        {
            ID = ID,
            Priority = Priority,
            Before = Before,
            After = After,
        }.ToReorg(GlobalIndex), applyByDefault: false);
        GC.SuppressFinalize(_hook);
    }

    private void Refresh()
    {
        if (_hook == null!) return;

        _hook.Dispose();
        Create();
        _hook.Apply();
    }

    public Detour(MethodBase from, MethodBase to, ref LegacyDetourConfig config)
        : this(from, to, null, config)
    {
    }

    public Detour(MethodBase from, MethodBase to, LegacyDetourConfig config)
        : this(from, to, ref config)
    {
    }

    public Detour(MethodBase from, MethodBase to)
        : this(from, to, LegacyDetourContext.Current?.DetourConfig ?? default)
    {
    }

    public Detour(MethodBase method, IntPtr to, ref LegacyDetourConfig config)
        : this(method, DetourHelper.GenerateNativeProxy(to, method), ref config)
    {
    }

    public Detour(MethodBase method, IntPtr to, LegacyDetourConfig config)
        : this(method, DetourHelper.GenerateNativeProxy(to, method), ref config)
    {
    }

    public Detour(MethodBase method, IntPtr to)
        : this(method, DetourHelper.GenerateNativeProxy(to, method))
    {
    }

    public Detour(Delegate from, IntPtr to, ref LegacyDetourConfig config)
        : this(from.Method, to, ref config)
    {
    }

    public Detour(Delegate from, IntPtr to, LegacyDetourConfig config)
        : this(from.Method, to, ref config)
    {
    }

    public Detour(Delegate from, IntPtr to)
        : this(from.Method, to)
    {
    }

    public Detour(Delegate from, Delegate to, ref LegacyDetourConfig config)
        : this(from.Method, to.Method, ref config)
    {
    }

    public Detour(Delegate from, Delegate to, LegacyDetourConfig config)
        : this(from.Method, to.Method, ref config)
    {
    }

    public Detour(Delegate from, Delegate to)
        : this(from.Method, to.Method)
    {
    }

    public Detour(Expression from, IntPtr to, ref LegacyDetourConfig config)
        : this(((MethodCallExpression) from).Method, to, ref config)
    {
    }

    public Detour(Expression from, IntPtr to, LegacyDetourConfig config)
        : this(((MethodCallExpression) from).Method, to, ref config)
    {
    }

    public Detour(Expression from, IntPtr to)
        : this(((MethodCallExpression) from).Method, to)
    {
    }

    public Detour(Expression from, Expression to, ref LegacyDetourConfig config)
        : this(((MethodCallExpression) from).Method, ((MethodCallExpression) to).Method, ref config)
    {
    }

    public Detour(Expression from, Expression to, LegacyDetourConfig config)
        : this(((MethodCallExpression) from).Method, ((MethodCallExpression) to).Method, ref config)
    {
    }

    public Detour(Expression from, Expression to)
        : this(((MethodCallExpression) from).Method, ((MethodCallExpression) to).Method)
    {
    }

    public Detour(Expression<Action> from, IntPtr to, ref LegacyDetourConfig config)
        : this(from.Body, to, ref config)
    {
    }

    public Detour(Expression<Action> from, IntPtr to, LegacyDetourConfig config)
        : this(from.Body, to, ref config)
    {
    }

    public Detour(Expression<Action> from, IntPtr to)
        : this(from.Body, to)
    {
    }

    public Detour(Expression<Action> from, Expression<Action> to, ref LegacyDetourConfig config)
        : this(from.Body, to.Body, ref config)
    {
    }

    public Detour(Expression<Action> from, Expression<Action> to, LegacyDetourConfig config)
        : this(from.Body, to.Body, ref config)
    {
    }

    public Detour(Expression<Action> from, Expression<Action> to)
        : this(from.Body, to.Body)
    {
    }

    public void Apply()
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(Detour));

        if (IsApplied)
            return;

        if (!(OnDetour?.InvokeWhileTrue(this, Method, Target) ?? true))
            return;

        _hook.Apply();
    }

    public void Undo()
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(Detour));

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

    private static readonly PropertyInfo s_getNextTrampoline
        = typeof(Hook).Assembly.GetType("MonoMod.RuntimeDetour.IDetour")?
              .GetProperty("NextTrampoline", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          ?? throw new InvalidOperationException("Couldn't get IDetour.NextTrampoline");

    private static readonly PropertyInfo s_getTrampolineMethod
        = typeof(Hook).Assembly.GetType("MonoMod.RuntimeDetour.IDetourTrampoline")?
              .GetProperty("TrampolineMethod", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          ?? throw new InvalidOperationException("Couldn't get IDetourTrampoline.TrampolineMethod");

    public MethodBase GenerateTrampoline(MethodBase? signature = null)
    {
        var remoteTrampoline = OnGenerateTrampoline?.InvokeWhileNull<MethodBase>(this, signature);
        if (remoteTrampoline != null)
            return remoteTrampoline;

        var trampoline = s_getNextTrampoline.GetValue(_hook);
        var trampolineMethod = (MethodBase) s_getTrampolineMethod.GetValue(trampoline)!;

        return trampolineMethod;
    }

    public T GenerateTrampoline<T>() where T : Delegate
    {
        if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
            throw new InvalidOperationException($"Type {typeof(T)} not a delegate type.");

        return GenerateTrampoline(typeof(T).GetMethod("Invoke")).CreateDelegate<T>();
    }
}
