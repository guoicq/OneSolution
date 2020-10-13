using System;
using System.IO;
using System.Reflection;

namespace OneSolution.Core.Utilities
{
    public class SysUtils
    {

        public static string ExecutableLocation => (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location;

        public static string ExecutableName => Path.GetFileName(ExecutableLocation);
        public static string ExecutableDirectory => Path.GetDirectoryName(ExecutableLocation);

        public static int CPUThreads => Environment.ProcessorCount;

        public static string MachineName => Environment.MachineName;
    }
}
