namespace MonoMod.RuntimeDetour.Platforms;

public class DetourNativeARMPlatform : IDetourNativePlatform
{
    public bool ShouldFlushICache;

    public DetourNativeARMPlatform()
    {
        throw new NotSupportedAnymoreException();
    }

    public NativeDetourData Create(IntPtr from, IntPtr to, byte? type)
    {
        throw new NotSupportedAnymoreException();
    }

    public void Free(NativeDetourData detour)
    {
        throw new NotSupportedAnymoreException();
    }

    public void Apply(NativeDetourData detour)
    {
        throw new NotSupportedAnymoreException();
    }

    public void Copy(IntPtr src, IntPtr dst, byte type)
    {
        throw new NotSupportedAnymoreException();
    }

    public void MakeWritable(IntPtr src, uint size)
    {
        throw new NotSupportedAnymoreException();
    }

    public void MakeExecutable(IntPtr src, uint size)
    {
        throw new NotSupportedAnymoreException();
    }

    public void MakeReadWriteExecutable(IntPtr src, uint size)
    {
        throw new NotSupportedAnymoreException();
    }

    public void FlushICache(IntPtr src, uint size)
    {
        throw new NotSupportedAnymoreException();
    }

    public IntPtr MemAlloc(uint size)
    {
        throw new NotSupportedAnymoreException();
    }

    public void MemFree(IntPtr ptr)
    {
        throw new NotSupportedAnymoreException();
    }

    public enum DetourType : byte
    {
        Thumb = 0,
        ThumbBX = 1,
        AArch32 = 2,
        AArch32BX = 3,
        AArch64 = 4,
    }
}
