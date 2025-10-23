using System.Linq.Expressions;
using System.Reflection;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

[UnbreakerReplace(typeof(Hook))]
public class LegacyHook : IDetour
{
    public static Func<Hook, MethodBase, MethodBase, object, bool>? OnDetour;
    public static Func<Hook, bool>? OnUndo;
    public static Func<Hook, MethodBase?, MethodBase>? OnGenerateTrampoline;

    public readonly MethodBase Method;
    public readonly MethodBase Target;
    public readonly MethodBase TargetReal;
    public readonly object DelegateTarget;

    public bool IsValid => Detour.IsValid;
    public bool IsApplied => Detour.IsApplied;
    public Detour Detour { get; }

    public LegacyHook(MethodBase from, MethodInfo to, object? target, ref HookConfig config)
    {
        from = from.GetIdentifiable();
        Method = from;
        Target = TargetReal = to;
        DelegateTarget = target;

        Detour = new Detour(Method, TargetReal, target, new LegacyDetourConfig
        {
            ManualApply = true,
            Priority = config.Priority,
            ID = config.ID,
            Before = config.Before,
            After = config.After,
        });

        if (!config.ManualApply)
            Apply();
    }

    public LegacyHook(MethodBase from, MethodInfo to, object? target, HookConfig config)
        : this(from, to, target, ref config)
    {
    }

    public LegacyHook(MethodBase from, MethodInfo to, object? target)
        : this(from, to, target, LegacyDetourContext.Current?.HookConfig ?? default)
    {
    }

    public LegacyHook(MethodBase from, MethodInfo to, ref HookConfig config)
        : this(from, to, null, ref config)
    {
    }

    public LegacyHook(MethodBase from, MethodInfo to, HookConfig config)
        : this(from, to, null, ref config)
    {
    }

    public LegacyHook(MethodBase from, MethodInfo to)
        : this(from, to, null)
    {
    }

    public LegacyHook(MethodBase method, IntPtr to, ref HookConfig config)
        : this(method, DetourHelper.GenerateNativeProxy(to, method), null, ref config)
    {
    }

    public LegacyHook(MethodBase method, IntPtr to, HookConfig config)
        : this(method, DetourHelper.GenerateNativeProxy(to, method), null, ref config)
    {
    }

    public LegacyHook(MethodBase method, IntPtr to)
        : this(method, DetourHelper.GenerateNativeProxy(to, method), null)
    {
    }

    public LegacyHook(MethodBase method, Delegate to, ref HookConfig config)
        : this(method, to.Method, to.Target, ref config)
    {
    }

    public LegacyHook(MethodBase method, Delegate to, HookConfig config)
        : this(method, to.Method, to.Target, ref config)
    {
    }

    public LegacyHook(MethodBase method, Delegate to)
        : this(method, to.Method, to.Target)
    {
    }

    public LegacyHook(Delegate from, IntPtr to, ref HookConfig config)
        : this(from.Method, to, ref config)
    {
    }

    public LegacyHook(Delegate from, IntPtr to, HookConfig config)
        : this(from.Method, to, ref config)
    {
    }

    public LegacyHook(Delegate from, IntPtr to)
        : this(from.Method, to)
    {
    }

    public LegacyHook(Delegate from, Delegate to, ref HookConfig config)
        : this(from.Method, to, ref config)
    {
    }

    public LegacyHook(Delegate from, Delegate to, HookConfig config)
        : this(from.Method, to, ref config)
    {
    }

    public LegacyHook(Delegate from, Delegate to)
        : this(from.Method, to)
    {
    }

    public LegacyHook(Expression from, IntPtr to, ref HookConfig config)
        : this(((MethodCallExpression) from).Method, to, ref config)
    {
    }

    public LegacyHook(Expression from, IntPtr to, HookConfig config)
        : this(((MethodCallExpression) from).Method, to, ref config)
    {
    }

    public LegacyHook(Expression from, IntPtr to)
        : this(((MethodCallExpression) from).Method, to)
    {
    }

    public LegacyHook(Expression from, Delegate to, ref HookConfig config)
        : this(((MethodCallExpression) from).Method, to, ref config)
    {
    }

    public LegacyHook(Expression from, Delegate to, HookConfig config)
        : this(((MethodCallExpression) from).Method, to, ref config)
    {
    }

    public LegacyHook(Expression from, Delegate to)
        : this(((MethodCallExpression) from).Method, to)
    {
    }

    public LegacyHook(Expression<Action> from, IntPtr to, ref HookConfig config)
        : this(from.Body, to, ref config)
    {
    }

    public LegacyHook(Expression<Action> from, IntPtr to, HookConfig config)
        : this(from.Body, to, ref config)
    {
    }

    public LegacyHook(Expression<Action> from, IntPtr to)
        : this(from.Body, to)
    {
    }

    public LegacyHook(Expression<Action> from, Delegate to, ref HookConfig config)
        : this(from.Body, to, ref config)
    {
    }

    public LegacyHook(Expression<Action> from, Delegate to, HookConfig config)
        : this(from.Body, to, ref config)
    {
    }

    public LegacyHook(Expression<Action> from, Delegate to)
        : this(from.Body, to)
    {
    }

    public void Apply()
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(Hook));

        if (!IsApplied && !(OnDetour?.InvokeWhileTrue(this, Method, Target, DelegateTarget) ?? true))
            return;

        Detour.Apply();
    }

    public void Undo()
    {
        if (!IsValid)
            throw new ObjectDisposedException(nameof(Hook));

        if (IsApplied && !(OnUndo?.InvokeWhileTrue(this) ?? true))
            return;

        Detour.Undo();
    }

    public void Free()
    {
        if (!IsValid)
            return;

        Detour.Free();
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

        return Detour.GenerateTrampoline(signature);
    }

    public T GenerateTrampoline<T>() where T : Delegate
    {
        if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
            throw new InvalidOperationException($"Type {typeof(T)} not a delegate type.");

        return GenerateTrampoline(typeof(T).GetMethod("Invoke")).CreateDelegate<T>();
    }
}
