using System;
using System.Linq;

namespace Deploy
{
    public class Installer
    {
        private const string name = "Path";
        private const EnvironmentVariableTarget target = EnvironmentVariableTarget.User;

        public static void Run()
        {
            var installDirectory = Environment.CurrentDirectory;

            if (GetPathDirectories().Any(dir => dir.Equals(installDirectory, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Already installed: {0}", installDirectory);
            }
            else
            {
                AddToPath(installDirectory);
                Console.WriteLine("Installed: {0}", installDirectory);
            }
        }

        private static string[] GetPathDirectories()
        {
            var value = Environment.GetEnvironmentVariable(name, target) ?? "";

            return value.Split(';');
        }

        private static void AddToPath(string directory)
        {
            var value = Environment.GetEnvironmentVariable(name, target) ?? "";

            if (value.Length > 0)
            {
                value += ";";
            }

            value += directory;

            Environment.SetEnvironmentVariable(name, value, target);
        }
    }
}
