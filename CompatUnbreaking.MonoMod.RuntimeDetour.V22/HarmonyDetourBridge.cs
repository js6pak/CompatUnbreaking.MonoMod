namespace MonoMod.RuntimeDetour;

public static class HarmonyDetourBridge
{
    public static bool Initialized => throw new NotSupportedAnymoreException();

    public static bool Init(bool forceLoad = true, Type type = Type.Auto)
    {
        throw new NotSupportedAnymoreException();
    }

    public static void Reset()
    {
        throw new NotSupportedAnymoreException();
    }

    public enum Type
    {
        Auto = 0,
        Basic = 1,
        AsOriginal = 2,
        Override = 3,
    }
}
