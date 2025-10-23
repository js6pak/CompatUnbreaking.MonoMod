namespace MonoMod.RuntimeDetour;

public struct HookConfig
{
    public bool ManualApply;
    public int Priority;
    public string ID;
    public IEnumerable<string> Before;
    public IEnumerable<string> After;
}
