using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod.Cil;

[UnbreakerExtensions]
public static class ILPatternMatchingExtExtensions
{
    extension(ILPatternMatchingExt)
    {
        public static bool MatchConv_OvfI([UnbreakerThis] Instruction instr) => instr.MatchConvOvfI();
        public static bool MatchConv_OvfU([UnbreakerThis] Instruction instr) => instr.MatchConvOvfU();
        public static bool MatchAdd_OvfUn([UnbreakerThis] Instruction instr) => instr.MatchAddOvfUn();

        public static bool MatchUnaligned([UnbreakerThis] Instruction instr, sbyte value)
        {
            return MatchUnaligned(instr, out sbyte v) && v == value;
        }

        public static bool MatchUnaligned([UnbreakerThis] Instruction instr, out sbyte value)
        {
            var result = instr.MatchUnaligned(out byte temporaryValue);
            value = (sbyte) temporaryValue;
            return result;
        }

        public static bool MatchNo([UnbreakerThis] Instruction instr, sbyte value)
        {
            return MatchNo(instr, out var v) && v == value;
        }

        public static bool MatchNo([UnbreakerThis] Instruction instr, out sbyte value)
        {
            if (Helpers.ThrowIfNull(instr).OpCode == OpCodes.No)
            {
                value = (sbyte) instr.Operand;
                return true;
            }

            value = 0;
            return false;
        }
    }
}
