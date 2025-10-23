using Mono.Cecil.Cil;

namespace MonoMod.Cil;

[UnbreakerExtensions]
public static class ILLabelExtensions
{
    extension(ILLabel @this)
    {
        [UnbreakerField]
        public Instruction? Target
        {
            get => @this.Target;
            set => @this.Target = value;
        }
    }
}
