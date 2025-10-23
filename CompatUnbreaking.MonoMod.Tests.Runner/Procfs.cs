using System.Globalization;

namespace CompatUnbreaking.MonoMod.Tests.Runner;

// Based on https://github.com/dotnet/runtime/blob/6da0fa9a95ab50d9dd14496f3625cf10eeffefab/src/libraries/Common/src/Interop/Linux/procfs/Interop.ProcFsStat.ParseMapModules.cs
// but without the "moduleHasReadAndExecFlags" check to avoid https://github.com/dotnet/runtime/issues/64042
internal static class Procfs
{
    private const uint SelfPid = 0;

    public record struct Module(
        string FileName,
        string ModuleName,
        IntPtr BaseAddress,
        int ModuleMemorySize
    );

    public static IEnumerable<Module> GetModules(uint pid = SelfPid)
    {
        Module? module = null;

        var mapsFilePath = pid == SelfPid
            ? "/proc/self/maps"
            : string.Create(null, stackalloc char[256], $"/proc/{pid}/maps");

        foreach (var line in File.ReadLines(mapsFilePath))
        {
            if (!TryParseMapsEntry(line, out var parsedLine))
            {
                // Invalid entry for the purposes of ProcessModule parsing,
                // discard flushing the current module if it exists.
                if (module is not null)
                {
                    yield return module.Value;
                    module = null;
                }

                continue;
            }

            // Check if entry is a continuation of the current module.
            if (module is not null &&
                module.Value.FileName == parsedLine.Path &&
                (long) module.Value.BaseAddress + module.Value.ModuleMemorySize == parsedLine.StartAddress)
            {
                // Is continuation, update the current module.
                module = module.Value with { ModuleMemorySize = module.Value.ModuleMemorySize + parsedLine.Size };
                continue;
            }

            // Not a continuation, commit any current modules and create a new one.
            if (module is not null)
            {
                yield return module.Value;
            }

            IntPtr baseAddress;
            unsafe
            {
                baseAddress = new IntPtr((void*) parsedLine.StartAddress);
            }

            module = new Module(
                parsedLine.Path,
                Path.GetFileName(parsedLine.Path),
                baseAddress,
                parsedLine.Size
            )
            {
                ModuleMemorySize = parsedLine.Size,
            };
        }

        if (module is not null)
        {
            yield return module.Value;
        }
    }

    private static bool TryParseMapsEntry(string line, out (long StartAddress, int Size, bool HasReadAndExecFlags, string Path) parsedLine)
    {
        // Use a StringParser to avoid string.Split costs
        var parser = new StringParser(line, separator: ' ', skipEmpty: true);

        // Parse the address start and size
        var (start, size) = parser.ParseRaw(TryParseAddressRange);

        if (size < 0)
        {
            parsedLine = default;
            return false;
        }

        // Parse the permissions
        var lineHasReadAndExecFlags = parser.ParseRaw(HasReadAndExecFlags);

        // Skip past the offset, dev, and inode fields
        parser.MoveNext();
        parser.MoveNext();
        parser.MoveNext();

        // we only care about the named modules
        if (!parser.MoveNext())
        {
            parsedLine = default;
            return false;
        }

        // Parse the pathname
        var pathname = parser.ExtractCurrentToEnd();
        parsedLine = (start, size, lineHasReadAndExecFlags, pathname);
        return true;

        static (long Start, int Size) TryParseAddressRange(string s, ref int start, ref int end)
        {
            var pos = s.IndexOf('-', start, end - start);
            if (pos > 0)
            {
                if (long.TryParse(s.AsSpan(start, pos), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var startingAddress) &&
                    long.TryParse(s.AsSpan(pos + 1, end - (pos + 1)), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var endingAddress))
                {
                    return (startingAddress, (int) (endingAddress - startingAddress));
                }
            }

            return (0, -1);
        }

        static bool HasReadAndExecFlags(string s, ref int start, ref int end)
        {
            var span = s.AsSpan(start, end - start);
            return span.Contains('r') && span.Contains('x');
        }
    }
}
