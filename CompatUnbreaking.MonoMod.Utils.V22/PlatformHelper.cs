namespace MonoMod.Utils;

public static class PlatformHelper
{
    private static readonly Platform s_current;

    public static Platform Current
    {
        get => s_current;
        set => throw new NotSupportedAnymoreException();
    }

    public static string LibrarySuffix { get; } =
                        Is(Platform.MacOS) ? "dylib" :
                        Is(Platform.Unix) ? "so" :
                        "dll";

    public static bool Is(Platform platform)
    {
        return (Current & platform) == platform;
    }

    static PlatformHelper()
    {
        s_current |= PlatformDetection.OS switch
        {
            OSKind.Android => Platform.Android,
            OSKind.Linux => Platform.Linux,
            OSKind.IOS => Platform.iOS,
            OSKind.OSX => Platform.MacOS,
            OSKind.BSD or OSKind.Posix => Platform.Unix,
            OSKind.Wine => Platform.Wine,
            OSKind.Windows => Platform.Windows,
            OSKind.Unknown => default,
        };

        if (PlatformDetection.Architecture.Has(ArchitectureKind.Bits64))
        {
            s_current |= Platform.Bits64;
        }
    }
}
