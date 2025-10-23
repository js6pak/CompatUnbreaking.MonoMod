using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoMod.Utils;

[UnbreakerExtensions]
public static class DynDllExtensions
{
    private static Dictionary<string, List<DynDllMapping>> s_mappings = new();

    extension(DynDll)
    {
        [UnbreakerField]
        public static Dictionary<string, List<DynDllMapping>> Mappings
        {
            get => s_mappings;
            set => s_mappings = value;
        }

        public static nint OpenLibrary(string name, bool skipMapping = false, int? flags = default)
        {
            if (flags != null) throw new NotSupportedAnymoreException();

            if (name != null && !skipMapping && DynDll.Mappings.TryGetValue(name, out var mappingList))
            {
                foreach (var mapping in mappingList)
                {
                    if (TryOpenLibrary(mapping.LibraryName, out var libraryPtr, true, mapping.Flags))
                        return libraryPtr;
                }

                return IntPtr.Zero;
            }

            return DynDll.OpenLibrary(name);
        }

        public static bool TryOpenLibrary(string name, out nint libraryPtr, bool skipMapping = false, int? flags = default)
        {
            if (flags != null) throw new NotSupportedAnymoreException();

            if (name != null && !skipMapping && DynDll.Mappings.TryGetValue(name, out var mappingList))
            {
                foreach (var mapping in mappingList)
                {
                    if (TryOpenLibrary(mapping.LibraryName, out libraryPtr, true, mapping.Flags))
                        return true;
                }

                libraryPtr = IntPtr.Zero;
                return true;
            }

            return DynDll.TryOpenLibrary(name, out libraryPtr);
        }

        public static bool CloseLibrary(nint lib)
        {
            return DynDll.TryCloseLibrary(lib);
        }

        public static nint GetFunction([UnbreakerThis] nint libraryPtr, string name)
        {
            return DynDll.GetExport(libraryPtr, name);
        }

        public static bool TryGetFunction([UnbreakerThis] nint libraryPtr, string name, out nint functionPtr)
        {
            return DynDll.TryGetExport(libraryPtr, name, out functionPtr);
        }

        public static T? AsDelegate<T>([UnbreakerThis] nint s) where T : class?
        {
            return Marshal.GetDelegateForFunctionPointer(s, typeof(T)) as T;
        }

        public static void ResolveDynDllImports([UnbreakerThis] Type type, Dictionary<string, List<DynDllMapping>>? mappings = null)
        {
            InternalResolveDynDllImports(type, null, mappings);
        }

        public static void ResolveDynDllImports(object instance, Dictionary<string, List<DynDllMapping>>? mappings = null)
        {
            InternalResolveDynDllImports(instance.GetType(), instance, mappings);
        }

        private static void InternalResolveDynDllImports(Type type, object? instance, Dictionary<string, List<DynDllMapping>>? mappings)
        {
            var fieldFlags = BindingFlags.Public | BindingFlags.NonPublic;
            if (instance == null)
                fieldFlags |= BindingFlags.Static;
            else
                fieldFlags |= BindingFlags.Instance;

            foreach (var field in type.GetFields(fieldFlags))
            {
                var found = true;

                foreach (DynDllImportAttribute attrib in field.GetCustomAttributes(typeof(DynDllImportAttribute), true))
                {
                    found = false;

                    var libraryPtr = IntPtr.Zero;

                    if (mappings != null && mappings.TryGetValue(attrib.LibraryName, out var mappingList))
                    {
                        var mappingFound = false;

                        foreach (var mapping in mappingList)
                        {
                            if (TryOpenLibrary(mapping.LibraryName, out libraryPtr, true, mapping.Flags))
                            {
                                mappingFound = true;
                                break;
                            }
                        }

                        if (!mappingFound)
                            continue;
                    }
                    else
                    {
                        if (!TryOpenLibrary(attrib.LibraryName, out libraryPtr))
                            continue;
                    }


                    foreach (var entryPoint in attrib.EntryPoints.Concat([field.Name, field.FieldType.Name]))
                    {
                        if (!TryGetFunction(libraryPtr, entryPoint, out var functionPtr))
                            continue;

#pragma warning disable CS0618 // Type or member is obsolete
                        field.SetValue(instance, Marshal.GetDelegateForFunctionPointer(functionPtr, field.FieldType));
#pragma warning restore CS0618 // Type or member is obsolete

                        found = true;
                        break;
                    }

                    if (found)
                        break;
                }

                if (!found)
                    throw new EntryPointNotFoundException($"No matching entry point found for {field.Name} in {field.DeclaringType.FullName}");
            }
        }
    }

    [UnbreakerExtension(typeof(DynDll))]
    public static class DynDllExtension
    {
        public static class DlopenFlags
        {
            public const int RTLD_LAZY = 0x0001;
            public const int RTLD_NOW = 0x0002;
            public const int RTLD_LOCAL = 0x0000;
            public const int RTLD_GLOBAL = 0x0100;
        }
    }
}
