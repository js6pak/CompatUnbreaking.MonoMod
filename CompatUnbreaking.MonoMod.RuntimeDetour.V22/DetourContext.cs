using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MonoMod.Utils;

namespace MonoMod.RuntimeDetour;

[UnbreakerReplace(typeof(DetourContext))]
public sealed class LegacyDetourContext : IDisposable
{
    [field: ThreadStatic]
    [field: AllowNull, MaybeNull]
    private static List<LegacyDetourContext> Contexts => field ??= [];

    [field: ThreadStatic]
    internal static LegacyDetourContext? Current
    {
        get
        {
            if (field?.IsValid ?? false)
                return field;

            var ctxs = Contexts;
            for (var i = ctxs.Count - 1; i > -1; i--)
            {
                var ctx = ctxs[i];
                if (!ctx.IsValid)
                    ctxs.RemoveAt(i);
                else
                    return field = ctx;
            }

            return null;
        }

        private set;
    }

    private readonly string _fallbackId;
    private readonly MethodBase? _creator;

    public int Priority;
    public List<string> Before;
    public List<string> After;

    [field: AllowNull, MaybeNull]
    public string ID
    {
        get => field ?? _fallbackId;
        set => field = string.IsNullOrEmpty(value) ? null : value;
    }

    public LegacyDetourConfig DetourConfig => new()
    {
        Priority = Priority,
        ID = ID,
        Before = Before,
        After = After,
    };

    public HookConfig HookConfig => new()
    {
        Priority = Priority,
        ID = ID,
        Before = Before,
        After = After,
    };

    public ILHookConfig ILHookConfig => new()
    {
        Priority = Priority,
        ID = ID,
        Before = Before,
        After = After,
    };

    private bool IsDisposed;

    internal bool IsValid
    {
        get
        {
            if (IsDisposed)
                return false;

            if (_creator == null)
                return true;

            StackTrace stack = new StackTrace();
            int frameCount = stack.FrameCount;

            for (int i = 0; i < frameCount; i++)
                if (stack.GetFrame(i).GetMethod() == _creator)
                    return true;

            return false;
        }
    }

    public LegacyDetourContext(int priority, string? id)
    {
        var stack = new StackTrace();
        var frameCount = stack.FrameCount;
        for (var i = 0; i < frameCount; i++)
        {
            var caller = stack.GetFrame(i)?.GetMethod();
            if (caller?.DeclaringType == typeof(LegacyDetourContext))
                continue;
            _creator = caller;
            break;
        }

        _fallbackId = _creator?.DeclaringType?.Assembly?.GetName().Name ?? _creator?.GetID(simple: true);

        Current = this;
        Contexts.Add(this);

        Priority = priority;
        ID = id;
    }

    public LegacyDetourContext(string id)
        : this(0, id)
    {
    }

    public LegacyDetourContext(int priority)
        : this(priority, null)
    {
    }

    public LegacyDetourContext()
        : this(0, null)
    {
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;
        IsDisposed = true;
        Current = null;
        Contexts.Remove(this);
    }
}
