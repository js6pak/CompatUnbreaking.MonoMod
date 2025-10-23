using System.Reflection;

namespace MonoMod.Cil;

/// <summary>
/// The default IL reference bag. Throws NotSupportedException for every operation.
/// </summary>
public sealed class NopILReferenceBag : IILReferenceBag
{
    public static NopILReferenceBag Instance = new();

    private Exception NOP() => new NotSupportedException("Inline references not supported in this context");

    public T Get<T>(int id) => throw NOP();
    public MethodInfo GetGetter<T>() => throw NOP();
    public int Store<T>(T t) => throw NOP();
    public void Clear<T>(int id) => throw NOP();
    public MethodInfo GetDelegateInvoker<T>() where T : Delegate => throw NOP();
}
