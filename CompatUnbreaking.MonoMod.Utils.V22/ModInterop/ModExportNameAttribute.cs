namespace MonoMod.ModInterop;

[UnbreakerExtensions]
public static class ModExportNameAttributeExtensions
{
    extension(ModExportNameAttribute @this)
    {
        [UnbreakerField]
        public string Name
        {
            get => @this.Name;
        }
    }
}
