namespace MonoMod.RuntimeDetour;

public struct ILHookConfig
{
    public bool ManualApply;
    public int Priority;
    public string? ID;
    public IEnumerable<string>? Before;
    public IEnumerable<string>? After;
}
