namespace MonoMod.Utils;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DynDllImportAttribute : Attribute
{
    public string LibraryName { get; set; }
    public string[] EntryPoints { get; set; }

    public DynDllImportAttribute(string libraryName, params string[] entryPoints)
    {
        LibraryName = libraryName;
        EntryPoints = entryPoints;
    }
}
