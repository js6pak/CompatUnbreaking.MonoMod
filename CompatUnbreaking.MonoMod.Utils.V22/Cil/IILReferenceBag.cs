using System.Reflection;

namespace MonoMod.Cil;

/// <summary>
/// An IL inline reference bag used for ILContexts.
/// </summary>
public interface IILReferenceBag
{
    /// <summary>
    /// Get the object for the given ID.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="id">The object ID.</param>
    /// <returns>The stored object.</returns>
    T? Get<T>(int id);

    /// <summary>
    /// Get a MethodInfo for the getter.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <returns>The getter method.</returns>
    MethodInfo GetGetter<T>();

    /// <summary>
    /// Store a new object.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="t">The object to be stored.</param>
    /// <returns>An ID to be used for all further operations.</returns>
    int Store<T>(T t);

    /// <summary>
    /// Remove the object with the given ID from the bag, essentially clearing the ID's slot.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="id">The object ID.</param>
    void Clear<T>(int id);

    /// <summary>
    /// Get a MethodInfo invoking a delegate of the given type, with the delegate at the top of the stack. Used by <see cref="ILCursor.EmitDelegate{T}(T)"/>.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <returns>A MethodInfo invoking a delegate of the given type.</returns>
    MethodInfo? GetDelegateInvoker<T>() where T : Delegate;
}
