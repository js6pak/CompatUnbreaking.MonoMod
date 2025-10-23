namespace MonoMod.Cil;

[UnbreakerExtensions]
public static class ILCursorExtensions
{
    extension(ILCursor @this)
    {
        public int AddReference<T>(T t)
        {
            return @this.AddReference(in t);
        }

        public int EmitReference<T>(T t)
        {
            return @this.EmitReference(in t);
        }
    }
}
