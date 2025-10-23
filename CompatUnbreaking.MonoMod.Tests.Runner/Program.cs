using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Versioning;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE;
using AsmResolver.PE.File;
using CompatUnbreaker;

namespace CompatUnbreaking.MonoMod.Tests.Runner;

internal sealed class Program
{
    public static int Main(string[] args)
    {
        var runtimeContext = new RuntimeContext(
            DotNetRuntimeInfo.Parse(Assembly.GetEntryAssembly()!.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName),
            new RuntimeAssemblyResolver(new ModuleReaderParameters())
        );

        Assembly.Load("MonoMod.Utils");
        Assembly.Load("MonoMod.RuntimeDetour");

        var testsAssembly = AssemblyDefinition.FromFile(Constants.TestsPath, runtimeContext.DefaultReaderParameters);

        Unbreaker.ProcessConsumer(CreateAssemblyDefinitionFromAssembly(Assembly.Load("CompatUnbreaking.MonoMod.Utils.V22"), runtimeContext.DefaultReaderParameters), testsAssembly);
        Unbreaker.ProcessConsumer(CreateAssemblyDefinitionFromAssembly(Assembly.Load("CompatUnbreaking.MonoMod.RuntimeDetour.V22"), runtimeContext.DefaultReaderParameters), testsAssembly);

        AppDomain.CurrentDomain.AssemblyResolve += static (_, args) =>
        {
            var assemblyName = new AssemblyName(args.Name);

            var assemblyPath = Path.Combine(Path.GetDirectoryName(Constants.TestsPath)!, assemblyName.Name + ".dll");

            try
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        };

        var tests = WriteAndLoad(testsAssembly);

        Assembly.SetEntryAssembly(tests);
        return ExecuteAssembly(tests, args);
    }

    private static Assembly WriteAndLoad(AssemblyDefinition assemblyDefinition)
    {
        Directory.CreateDirectory("out");
        var path = Path.Combine("out", assemblyDefinition.Name + ".dll");
        assemblyDefinition.Write(path);
        return Assembly.LoadFrom(path);
    }

    private static int ExecuteAssembly(Assembly assembly, string?[]? args)
    {
        var entry = assembly.EntryPoint ??
                    throw new MissingMethodException("Entry point was not found.");

        var result = entry.Invoke(
            obj: null,
            invokeAttr: BindingFlags.DoNotWrapExceptions,
            binder: null,
            parameters: entry.GetParameters().Length > 0 ? [args] : null,
            culture: null
        );

        return result != null ? (int) result : 0;
    }

    private static AssemblyDefinition CreateAssemblyDefinitionFromAssembly(Assembly assembly, ModuleReaderParameters readerParameters)
    {
        if (OperatingSystem.IsLinux())
        {
            foreach (var module in Procfs.GetModules())
            {
                if (Path.GetFileNameWithoutExtension(module.ModuleName) == assembly.GetName().Name)
                {
                    return AssemblyDefinition.FromImage(PEImage.FromModuleBaseAddress(module.BaseAddress, PEMappingMode.Unmapped, new PEReaderParameters()), readerParameters);
                }
            }
        }

        if (!string.IsNullOrEmpty(assembly.Location))
        {
            return AssemblyDefinition.FromFile(assembly.Location, readerParameters);
        }

        throw new NotSupportedException();
    }
}

internal sealed class RuntimeAssemblyResolver : IAssemblyResolver
{
    private readonly ConcurrentDictionary<AssemblyDescriptor, AssemblyDefinition?> _cache = new(SignatureComparer.Default);

    public RuntimeAssemblyResolver(ModuleReaderParameters readerParameters)
    {
        ReaderParameters = readerParameters;
    }

    public ModuleReaderParameters ReaderParameters { get; }

    public AssemblyDefinition? Resolve(AssemblyDescriptor descriptor)
    {
        return _cache.GetOrAdd(descriptor, static descriptor =>
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == descriptor.Name);
            if (assembly == null) return null;

            if (string.IsNullOrEmpty(assembly.Location))
            {
                throw new NotSupportedException();
            }

            return AssemblyDefinition.FromFile(assembly.Location);
        });
    }

    public void AddToCache(AssemblyDescriptor descriptor, AssemblyDefinition definition)
    {
        if (_cache.ContainsKey(descriptor))
            throw new ArgumentException($"The cache already contains an entry of assembly {descriptor.FullName}.", nameof(descriptor));

        _cache.TryAdd(descriptor, definition);
    }

    public bool RemoveFromCache(AssemblyDescriptor descriptor) => _cache.TryRemove(descriptor, out _);
    public bool HasCached(AssemblyDescriptor descriptor) => _cache.ContainsKey(descriptor);
    public void ClearCache() => _cache.Clear();
}
