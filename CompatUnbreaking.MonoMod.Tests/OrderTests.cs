using System.Reflection;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace CompatUnbreaking.MonoMod.Tests;

public class OrderTests(ITestOutputHelper output)
{
    [Fact]
    public void TestDetoursOrder()
    {
        var original = ((Delegate) Foo).Method;

        var actual = new List<string>();

        using (ILHook("IL A"))
        using (Hook("A"))
        using (ILHook("IL B"))
        using (var b = Hook("B", manualApply: true))
        using (Hook("C"))
        using (Hook("D"))
        using (ILHook("IL C"))
        using (Hook("Last", priority: int.MinValue))
        using (Hook("First", priority: int.MaxValue))
        using (Hook("BeforeA", before: ["A"]))
        using (Hook("AfterA", after: ["A"]))
        using (Hook("BeforeAll", before: ["*"]))
        using (Hook("AfterAll", after: ["*"]))
        {
            b.Apply();

            Foo();

            output.WriteLine(string.Join(", ", actual));

            // TODO pre-reorg ordering is cooked
            var isReorg = Assembly.Load("MonoMod.RuntimeDetour").GetName().Version!.Major > 22;
            if (isReorg)
            {
                Assert.Equal(
                    "First, AfterAll, AfterA, A, BeforeA, D, C, B, BeforeAll, Last, IL C, IL B, IL A",
                    string.Join(", ", actual)
                );
            }
        }

        Hook Hook(string id, int priority = 0, IEnumerable<string>? before = null, IEnumerable<string>? after = null, bool manualApply = false)
        {
            var config = new HookConfig
            {
                ID = id,
                Priority = priority,
                Before = before,
                After = after,
                ManualApply = manualApply,
            };

            return new Hook(original, (Action orig) =>
            {
                actual.Add(id);
                orig();
            }, config);
        }

        ILHook ILHook(string id, int priority = 0, IEnumerable<string>? before = null, IEnumerable<string>? after = null, bool manualApply = false)
        {
            var config = new ILHookConfig
            {
                ID = id,
                Priority = priority,
                Before = before,
                After = after,
                ManualApply = manualApply,
            };

            return new ILHook(original, il =>
            {
                var c = new ILCursor(il);
                c.EmitDelegate(void () => actual.Add(id));
            }, config);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Foo()
    {
    }
}
