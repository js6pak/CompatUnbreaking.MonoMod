namespace MonoMod.Utils.Cil;

[UnbreakerExtensions]
public static class ILGeneratorShimExtensions
{
    extension(ILGeneratorShim @this)
    {
        public static Type ProxyType => ILGeneratorShim.GenericProxyType;
    }
}
