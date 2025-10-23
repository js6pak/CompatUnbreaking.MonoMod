using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace CompatUnbreaking.MonoMod.Tests;

public sealed class ShimTests(ITestOutputHelper output)
{
    private int Sum(int a, int b) => a + b;

    private MethodInfo SumMethod => ((Delegate) Sum).Method;

    [Fact]
    public void ReflectionHelperTest()
    {
        output.WriteLine("ReflectionHelper.IsMono: " + ReflectionHelper.IsMono);
        output.WriteLine("ReflectionHelper.IsCore: " + ReflectionHelper.IsCore);
    }

    [Fact]
    public void FastDelegateTest()
    {
        var fastDelegate = SumMethod.GetFastDelegate();
        Assert.Equal(3, fastDelegate(this, 1, 2));
    }

    [Fact]
    public void ReferenceBagTest()
    {
        Assert.ThrowsAny<Exception>(() =>
        {
            RuntimeILReferenceBag.InnerBag<string>.Get(100);
        });

        var o = "hello";

        var id = RuntimeILReferenceBag.InnerBag<string>.Store(o);
        Assert.Equal(o, RuntimeILReferenceBag.InnerBag<string>.Get(id));

        RuntimeILReferenceBag.InnerBag<string>.Clear(id);
        Assert.Null(RuntimeILReferenceBag.InnerBag<string>.Get(id));
    }

    [Fact]
    public void DynamicMethodDefinitionTest()
    {
        using var dynamicMethodDefinition = new DynamicMethodDefinition(SumMethod);

#pragma warning disable CS0618 // Type or member is obsolete
        Assert.Equal(dynamicMethodDefinition.Method, DMDNoopGenerator.Generate(dynamicMethodDefinition));
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private sealed class DMDNoopGenerator : DMDGenerator<DMDNoopGenerator>
    {
        protected override MethodInfo _Generate(DynamicMethodDefinition dmd, object? context)
        {
            return (MethodInfo) dmd.Method;
        }
    }

    [Fact]
    public void HookTest()
    {
        using var hook = new Hook(
            SumMethod,
            (object @this, int a, int b) => a - b
        );

        Assert.Equal(-1, Sum(1, 2));
    }
}
