using System.Reflection;

namespace MonoMod.Utils;

[UnbreakerExtensions]
public static class DynamicMethodDefinitionExtension
{
    private static readonly FieldInfo s_nameField =
        typeof(DynamicMethodDefinition).GetField("<Name>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly Action<DynamicMethodDefinition, bool> s_setDebug =
        typeof(DynamicMethodDefinition).GetProperty(nameof(DynamicMethodDefinition.Debug))!
            .GetSetMethod(true)!
            .CreateDelegate<Action<DynamicMethodDefinition, bool>>();

    extension(DynamicMethodDefinition @this)
    {
        public MethodBase? Method => @this.OriginalMethod;

        [UnbreakerField]
        public string Name
        {
            get => @this.Name;
            set => s_nameField.SetValue(@this, value);
        }

        [UnbreakerField]
        public Type OwnerType
        {
            get => throw new NotSupportedAnymoreException();
            set => throw new NotSupportedAnymoreException();
        }

        [UnbreakerField]
        public bool Debug
        {
            get => @this.Debug;
            set => s_setDebug(@this, value);
        }

        public void Reload()
        {
        }
    }
}
