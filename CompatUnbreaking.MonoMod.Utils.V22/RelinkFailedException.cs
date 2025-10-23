using System.Reflection;
using Mono.Cecil;

namespace MonoMod.Utils;

[UnbreakerExtensions]
public static class RelinkFailedExceptionExtensions
{
    private static readonly Func<string, IMetadataTokenProvider, IMetadataTokenProvider, string> s_format =
        typeof(RelinkFailedException).GetMethod("Format")!
            .CreateDelegate<Func<string, IMetadataTokenProvider, IMetadataTokenProvider, string>>();

    extension(RelinkFailedException @this)
    {
        [UnbreakerField]
        public IMetadataTokenProvider MTP => @this.MTP;

        [UnbreakerField]
        public IMetadataTokenProvider? Context => @this.Context;

        public static string _Format(string message, IMetadataTokenProvider mtp, IMetadataTokenProvider context)
        {
            return s_format(message, mtp, context);
        }
    }
}
