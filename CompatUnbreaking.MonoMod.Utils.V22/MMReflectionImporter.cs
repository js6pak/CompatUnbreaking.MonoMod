namespace MonoMod.Utils;

[UnbreakerExtensions]
public static class MMReflectionImporterExtensions
{
    extension(MMReflectionImporter @this)
    {
        [UnbreakerField]
        public bool UseDefault
        {
            get => @this.UseDefault;
            set => @this.UseDefault = value;
        }
    }
}
