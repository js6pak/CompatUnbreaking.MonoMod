namespace MonoMod.Utils;

[UnbreakerExtensions]
public static class ReflectionHelperExtensions
{
    extension(ReflectionHelper)
    {
        [UnbreakerField]
        public static bool IsMono => PlatformDetection.Runtime == RuntimeKind.Mono;

        [UnbreakerField]
        public static bool IsCore => PlatformDetection.Runtime == RuntimeKind.CoreCLR;
    }
}
