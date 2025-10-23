using Mono.Cecil.Cil;

namespace MonoMod.Utils.Cil;

[UnbreakerExtensions]
public static class CecilILGeneratorExtensions
{
    extension(CecilILGenerator @this)
    {
        [UnbreakerField]
        public ILProcessor IL => @this.IL;
    }
}
