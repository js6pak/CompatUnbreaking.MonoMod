namespace MonoMod.RuntimeDetour;

public struct NativeDetourData
{
    public IntPtr Method;
    public IntPtr Target;
    public byte Type;
    public uint Size;
    public IntPtr Extra;
}
