using System.Dynamic;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

public sealed class DynamicHookGen : DynamicObject
{
    private DynamicHookGen Parent;
    private string Name;
    private Type Type;

    private string Path
    {
        get
        {
            if (Parent?.Name == null)
                return Name;

            var list = new List<string>();
            for (var node = this; node?.Name != null; node = node.Parent)
            {
                list.Add(node.Name);
            }

            list.Reverse();
            return string.Join(".", list);
        }
    }

    private HookType NodeHookType;

    public static dynamic On = new DynamicHookGen(HookType.On);
    public static dynamic IL = new DynamicHookGen(HookType.IL);
    public static dynamic OnOrIL = new DynamicHookGen(HookType.OnOrIL);

    private int OwnLendID;
    private int NextLendID;

    private List<Tuple<ActionType, Delegate>> Actions = [];

    private DynamicHookGen(HookType hookType)
    {
        NodeHookType = hookType;
    }

    private DynamicHookGen(DynamicHookGen parent, string name)
    {
        Parent = parent;
        Name = name;
        NodeHookType = parent.NodeHookType;
        OwnLendID = parent.NextLendID++;
    }

    private DynamicHookGen(DynamicHookGen source)
    {
        Parent = source.Parent;
        Name = source.Name;
        NodeHookType = source.NodeHookType;
        OwnLendID = source.OwnLendID;
        Actions.AddRange(source.Actions);
    }

    public DynamicHookGen(Type type)
        : this(type, HookType.OnOrIL)
    {
    }

    public DynamicHookGen(Type type, HookType hookType)
    {
        Name = type.FullName;
        Type = type;
        NodeHookType = hookType;
    }

    [Obsolete]
    private void Apply()
    {
        var typeName = Parent.Path;
        var type = Parent.Type ?? ReflectionHelper.GetType(typeName);
        if (type == null)
            throw new ArgumentException($"Couldn't find type {typeName}");

        MethodBase method;
        // TODO: Handle overloads.
        if (Name == "ctor")
            method = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();
        else
            method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault(m => m.Name == Name);
        if (method == null)
            throw new ArgumentException($"Couldn't find method {typeName}::{Name}");

        foreach (var action in Actions)
        {
            var target = action.Item2;

            var hookType = NodeHookType;

            if (hookType == HookType.OnOrIL)
            {
                var invoke = target.GetType().GetMethod("Invoke");
                var args = invoke.GetParameters();

                if (invoke.ReturnType == typeof(void) &&
                    args.Length == 1 &&
                    args[0].ParameterType.IsCompatible(typeof(ILContext)) &&
                    !args[0].IsOut)
                {
                    hookType = HookType.IL;
                }
                else
                {
                    hookType = HookType.On;
                }
            }

            switch (action.Item1)
            {
                case ActionType.Add:
                    if (hookType == HookType.IL)
                        HookEndpointManager.Modify(method, target);
                    else
                        HookEndpointManager.Add(method, target);
                    break;

                case ActionType.Remove:
                    if (hookType == HookType.IL)
                        HookEndpointManager.Unmodify(method, target);
                    else
                        HookEndpointManager.Remove(method, target);
                    break;
            }
        }

        Actions.Clear();
    }

    public override bool TryInvoke(InvokeBinder binder, object?[]? args, out object result)
    {
        if (args.Length != 1 ||
            args[0] is not Type type)
        {
            throw new ArgumentException("Expected type.");
        }

        result = new DynamicHookGen(type, NodeHookType);
        return true;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        result = new DynamicHookGen(this, binder.Name);
        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if (value is not DynamicHookGen child)
            throw new ArgumentException("Incompatible dynamic hooks type. Did you use += / -= properly?");

        if (child.Parent != this)
            throw new ArgumentException("Dynamic hooks target parent not matching.");

        if (child.Name != binder.Name)
            throw new ArgumentException("Dynamic hooks target name not matching.");

        if (child.OwnLendID != NextLendID++ - 1)
            throw new ArgumentException("Dynamic hooks object expired.");

        child.Apply();
        return true;
    }

    public static DynamicHookGen operator +(DynamicHookGen ctx, Delegate target)
    {
        ctx = new DynamicHookGen(ctx);
        ctx.Actions.Add(new Tuple<ActionType, Delegate>(ActionType.Add, target));
        return ctx;
    }

    public static DynamicHookGen operator -(DynamicHookGen ctx, Delegate target)
    {
        ctx = new DynamicHookGen(ctx);
        ctx.Actions.Add(new Tuple<ActionType, Delegate>(ActionType.Remove, target));
        return ctx;
    }

    private enum ActionType
    {
        Add,
        Remove,
    }

    public enum HookType
    {
        OnOrIL,
        On,
        IL,
    }
}
