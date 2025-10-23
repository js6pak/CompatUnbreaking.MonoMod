using System.Linq.Expressions;

namespace MonoMod.RuntimeDetour;

public class Hook<TFrom, TTo> : LegacyHook
{
    public Hook(Expression<Func<TFrom>> from, TTo to, ref HookConfig config)
        : base(from.Body, to as Delegate)
    {
    }

    public Hook(Expression<Func<TFrom>> from, TTo to, HookConfig config)
        : base(from.Body, to as Delegate)
    {
    }

    public Hook(Expression<Func<TFrom>> from, TTo to)
        : base(from.Body, to as Delegate)
    {
    }

    public Hook(TFrom from, TTo to, ref HookConfig config)
        : base(from as Delegate, to as Delegate)
    {
    }

    public Hook(TFrom from, TTo to, HookConfig config)
        : base(from as Delegate, to as Delegate)
    {
    }

    public Hook(TFrom from, TTo to)
        : base(from as Delegate, to as Delegate)
    {
    }
}
