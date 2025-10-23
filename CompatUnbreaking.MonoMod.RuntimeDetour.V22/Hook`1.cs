using System.Linq.Expressions;

namespace MonoMod.RuntimeDetour;

public class Hook<T> : LegacyHook
{
    public Hook(Expression<Action> from, T to, ref HookConfig config)
        : base(from.Body, to as Delegate, ref config)
    {
    }

    public Hook(Expression<Action> from, T to, HookConfig config)
        : base(from.Body, to as Delegate, ref config)
    {
    }

    public Hook(Expression<Action> from, T to)
        : base(from.Body, to as Delegate)
    {
    }

    public Hook(Expression<Func<T>> from, IntPtr to, ref HookConfig config)
        : base(from.Body, to, ref config)
    {
    }

    public Hook(Expression<Func<T>> from, IntPtr to, HookConfig config)
        : base(from.Body, to, ref config)
    {
    }

    public Hook(Expression<Func<T>> from, IntPtr to)
        : base(from.Body, to)
    {
    }

    public Hook(Expression<Func<T>> from, Delegate to, ref HookConfig config)
        : base(from.Body, to, ref config)
    {
    }

    public Hook(Expression<Func<T>> from, Delegate to, HookConfig config)
        : base(from.Body, to, ref config)
    {
    }

    public Hook(Expression<Func<T>> from, Delegate to)
        : base(from.Body, to)
    {
    }

    public Hook(T from, IntPtr to, ref HookConfig config)
        : base(from as Delegate, to, ref config)
    {
    }

    public Hook(T from, IntPtr to, HookConfig config)
        : base(from as Delegate, to, ref config)
    {
    }

    public Hook(T from, IntPtr to)
        : base(from as Delegate, to)
    {
    }

    public Hook(T from, T to, ref HookConfig config)
        : base(from as Delegate, to as Delegate, ref config)
    {
    }

    public Hook(T from, T to, HookConfig config)
        : base(from as Delegate, to as Delegate, ref config)
    {
    }

    public Hook(T from, T to)
        : base(from as Delegate, to as Delegate)
    {
    }
}
