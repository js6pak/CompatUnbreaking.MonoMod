namespace MonoMod.Cil;

[UnbreakerExtensions]
public static class ILContextExtensions
{
    extension(ILContext @this)
    {
        [UnbreakerField]
        public IILReferenceBag ReferenceBag
        {
            get => RuntimeILReferenceBag.Instance;
            set
            {
                if (value != RuntimeILReferenceBag.Instance)
                {
                    throw new NotSupportedAnymoreException();
                }
            }
        }

        public int AddReference<T>(T t)
        {
            return @this.AddReference(in t);
        }
    }
}
