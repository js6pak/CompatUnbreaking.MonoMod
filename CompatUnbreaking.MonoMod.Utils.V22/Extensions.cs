using System.Reflection;

namespace MonoMod.Utils;

[UnbreakerExtensions]
public static class ExtensionsExtensions
{
    extension(Extensions)
    {
        /// <summary>
        /// Print the exception to the console, including extended loading / reflection data useful for mods.
        /// </summary>
        public static void LogDetailed([UnbreakerThis] Exception e, string? tag = null)
        {
            if (tag == null)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine("Detailed exception log:");
            }

            for (var e2 = e; e2 != null; e2 = e2.InnerException)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine(e2.GetType().FullName + ": " + e2.Message + "\n" + e2.StackTrace);
                if (e2 is ReflectionTypeLoadException rtle)
                {
                    for (var i = 0; i < rtle.Types.Length; i++)
                    {
                        Console.WriteLine("ReflectionTypeLoadException.Types[" + i + "]: " + rtle.Types[i]);
                    }

                    for (var i = 0; i < rtle.LoaderExceptions.Length; i++)
                    {
                        LogDetailed(rtle.LoaderExceptions[i], tag + (tag == null ? string.Empty : ", ") + "rtle:" + i);
                    }
                }

                if (e2 is TypeLoadException typeLoadException)
                {
                    Console.WriteLine("TypeLoadException.TypeName: " + typeLoadException.TypeName);
                }

                if (e2 is BadImageFormatException badImageFormatException)
                {
                    Console.WriteLine("BadImageFormatException.FileName: " + badImageFormatException.FileName);
                }
            }
        }
    }
}
