namespace MonoMod.ModInterop;

[UnbreakerExtensions]
public static class ModImportNameAttributeExtensions
{
    extension(ModImportNameAttribute @this)
    {
        [UnbreakerField]
        public string Name
        {
            get => @this.Name;
        }
    }
}
