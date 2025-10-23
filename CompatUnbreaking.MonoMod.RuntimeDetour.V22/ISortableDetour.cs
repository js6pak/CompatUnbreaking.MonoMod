namespace MonoMod.RuntimeDetour;

public interface ISortableDetour : IDetour
{
    uint GlobalIndex { get; }
    int Priority { get; set; }
    string ID { get; set; }
    IEnumerable<string> Before { get; set; }
    IEnumerable<string> After { get; set; }
}
