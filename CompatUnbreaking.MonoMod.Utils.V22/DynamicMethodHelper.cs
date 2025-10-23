using System.Reflection;
using CIL = Mono.Cecil.Cil;
using SRE = System.Reflection.Emit;
namespace MonoMod.Utils;

public static class DynamicMethodHelper
{
    private static readonly List<object?> s_references = [];

    public static object? GetReference(int id) => s_references[id];
    public static void SetReference(int id, object? obj) => s_references[id] = obj;
    private static int AddReference(object? obj)
    {
        lock (s_references)
        {
            s_references.Add(obj);
            return s_references.Count - 1;
        }
    }
    public static void FreeReference(int id) => s_references[id] = null;

    private static readonly MethodInfo s_getReference = typeof(DynamicMethodHelper).GetMethod(nameof(GetReference))!;

    /// <summary>
    /// Fill the DynamicMethod with a stub.
    /// </summary>
    public static SRE.DynamicMethod Stub(this SRE.DynamicMethod dm)
    {
        var il = dm.GetILGenerator();
        for (var i = 0; i < 32; i++)
        {
            // Prevent mono from inlining the DynamicMethod.
            il.Emit(SRE.OpCodes.Nop);
        }
        if (dm.ReturnType != typeof(void))
        {
            il.DeclareLocal(dm.ReturnType);
            il.Emit(SRE.OpCodes.Ldloca_S, (sbyte) 0);
            il.Emit(SRE.OpCodes.Initobj, dm.ReturnType);
            il.Emit(SRE.OpCodes.Ldloc_0);
        }
        il.Emit(SRE.OpCodes.Ret);
        return dm;
    }

    /// <summary>
    /// Fill the DynamicMethod with a stub.
    /// </summary>
    public static DynamicMethodDefinition Stub(this DynamicMethodDefinition dmd)
    {
        var il = dmd.GetILProcessor();
        for (var i = 0; i < 32; i++)
        {
            // Prevent mono from inlining the DynamicMethod.
            il.Emit(CIL.OpCodes.Nop);
        }
        if (dmd.Definition.ReturnType != dmd.Definition.Module.TypeSystem.Void)
        {
            il.Body.Variables.Add(new CIL.VariableDefinition(dmd.Definition.ReturnType));
            il.Emit(CIL.OpCodes.Ldloca_S, (sbyte) 0);
            il.Emit(CIL.OpCodes.Initobj, dmd.Definition.ReturnType);
            il.Emit(CIL.OpCodes.Ldloc_0);
        }
        il.Emit(CIL.OpCodes.Ret);
        return dmd;
    }

    /// <summary>
    /// Emit a reference to an arbitrary object. Note that the references "leak."
    /// </summary>
    public static int EmitReference<T>(this SRE.ILGenerator il, T obj)
    {
        var t = typeof(T);
        var id = AddReference(obj);
        il.Emit(SRE.OpCodes.Ldc_I4, id);
        il.Emit(SRE.OpCodes.Call, s_getReference);
        if (t.IsValueType)
            il.Emit(SRE.OpCodes.Unbox_Any, t);
        return id;
    }

    /// <summary>
    /// Emit a reference to an arbitrary object. Note that the references "leak."
    /// </summary>
    public static int EmitReference<T>(this CIL.ILProcessor il, T obj)
    {
        var ilModule = il.Body.Method.Module;
        var t = typeof(T);
        var id = AddReference(obj);
        il.Emit(CIL.OpCodes.Ldc_I4, id);
        il.Emit(CIL.OpCodes.Call, ilModule.ImportReference(s_getReference));
        if (t.IsValueType)
            il.Emit(CIL.OpCodes.Unbox_Any, ilModule.ImportReference(t));
        return id;
    }

    /// <summary>
    /// Emit a reference to an arbitrary object. Note that the references "leak."
    /// </summary>
    public static int EmitGetReference<T>(this SRE.ILGenerator il, int id)
    {
        var t = typeof(T);
        il.Emit(SRE.OpCodes.Ldc_I4, id);
        il.Emit(SRE.OpCodes.Call, s_getReference);
        if (t.IsValueType)
            il.Emit(SRE.OpCodes.Unbox_Any, t);
        return id;
    }

    /// <summary>
    /// Emit a reference to an arbitrary object. Note that the references "leak."
    /// </summary>
    public static int EmitGetReference<T>(this CIL.ILProcessor il, int id)
    {
        var ilModule = il.Body.Method.Module;
        var t = typeof(T);
        il.Emit(CIL.OpCodes.Ldc_I4, id);
        il.Emit(CIL.OpCodes.Call, ilModule.ImportReference(s_getReference));
        if (t.IsValueType)
            il.Emit(CIL.OpCodes.Unbox_Any, ilModule.ImportReference(t));
        return id;
    }
}
