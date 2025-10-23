namespace MonoMod.Utils;

[Flags]
public enum Platform
{
    OS = 1 << 0,

    Bits64 = 1 << 1,

    NT = 1 << 2,
    Unix = 1 << 3,

    ARM = 1 << 16,

    Wine = 1 << 17,

    Unknown = OS | (1 << 4),
    Windows = OS | NT | (1 << 5),
    MacOS = OS | Unix | (1 << 6),
    Linux = OS | Unix | (1 << 7),
    Android = Linux | (1 << 8),
    iOS = MacOS | (1 << 9),
}
