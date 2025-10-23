namespace MonoMod.RuntimeDetour;

public class Detour<T> : Detour where T : Delegate
{
    public Detour(T from, IntPtr to, ref LegacyDetourConfig config)
        : base(from, to, ref config)
    {
    }

    public Detour(T from, IntPtr to, LegacyDetourConfig config)
        : base(from, to, ref config)
    {
    }

    public Detour(T from, IntPtr to)
        : base(from, to)
    {
    }

    public Detour(T from, T to, ref LegacyDetourConfig config)
        : base(from, to, ref config)
    {
    }

    public Detour(T from, T to, LegacyDetourConfig config)
        : base(from, to, ref config)
    {
    }

    public Detour(T from, T to)
        : base(from, to)
    {
    }
}
