using System.Reflection;
using MonoMod.Utils;
using _FastDelegateInvokers = MonoMod.Cil.FastDelegateInvokers;

namespace MonoMod.Cil;

/// <summary>
/// An IL reference bag implementation to be used for runtime-generated methods.
/// </summary>
public sealed partial class RuntimeILReferenceBag : IILReferenceBag
{
    public static RuntimeILReferenceBag Instance = new();

    public T? Get<T>(int id) => InnerBag<T>.Get(id);
    public MethodInfo GetGetter<T>() => InnerBag<T>.Getter;
    public int Store<T>(T t) => InnerBag<T>.Store(t);
    public void Clear<T>(int id) => InnerBag<T>.Clear(id);

    public MethodInfo? GetDelegateInvoker<T>() where T : Delegate
    {
        return _FastDelegateInvokers.GetDelegateInvoker(typeof(T))?.Invoker;
    }

    public static class InnerBag<T>
    {
        private static readonly List<DataScope<DynamicReferenceCell>> s_managedObjectRefs = [];

        public static T? Get(int id)
        {
            if (id < 0 || id >= s_managedObjectRefs.Count)
                throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                return DynamicReferenceManager.GetValue<T>(s_managedObjectRefs[id].Data);
            }
            catch (ArgumentException e) when (e.Message.StartsWith("Referenced cell no longer exists", StringComparison.Ordinal))
            {
                return default;
            }
        }

        [UnbreakerField]
        public static MethodInfo Getter { get; } = typeof(InnerBag<T>).GetMethod(nameof(Get));

        public static int Store(T t)
        {
            var id = s_managedObjectRefs.Count;
            var scope = DynamicReferenceManager.AllocReference(in t, out _);
            s_managedObjectRefs.Add(scope);
            return id;
        }

        public static void Clear(int id)
        {
            s_managedObjectRefs[id].Dispose();
            s_managedObjectRefs[id] = default;
        }
    }
}
