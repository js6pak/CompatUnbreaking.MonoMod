namespace MonoMod.RuntimeDetour;

[UnbreakerReplace(typeof(DetourConfig))]
public struct LegacyDetourConfig
{
    public bool ManualApply;
    public int Priority;
    public string? ID;
    public IEnumerable<string>? Before;
    public IEnumerable<string>? After;

    public readonly DetourConfig? ToReorg(uint globalIndex, bool isILHook = false)
    {
        var priority = Priority;

        if (Before != null && Before.Contains("*"))
        {
            priority += int.MinValue / 10;
        }
        else if (After != null && After.Contains("*"))
        {
            priority += int.MaxValue / 10;
        }

        if (isILHook)
        {
            priority = unchecked(int.MaxValue - (priority - int.MinValue));
            globalIndex = uint.MaxValue - globalIndex;
        }

        return new DetourConfig(
            ID,
            priority,
            // what
            before: !isILHook ? After : Before,
            after: !isILHook ? Before : After,
            subPriority: unchecked(int.MinValue + (int) globalIndex)
        );
    }
}
