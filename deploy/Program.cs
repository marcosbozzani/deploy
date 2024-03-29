﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Deploy
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //args = new[] { @"..\..\..\sample\deploy" };
            //args = new[] { "--init" };
            //args = new[] { "--help" };
            //args = new[] { "--install" };
            args = new[] { "--version" };
#endif
            if (args.Length == 1 && args[0] == "--init")
            {
                InitConfig.Create();
            }
            else if (args.Length == 1 && args[0] == "--install")
            {
                Installer.Run();
            }
            else if (args.Length == 1 && args[0] == "--version")
            {
                ShowVersion();
            }
            else if (args.Length == 1 && args[0] == "--help")
            {
                ShowHelp();
            }
            else
            {
                var options = Options.Load(args);

                using (var localShell = new LocalShell())
                using (var remoteShell = new RemoteShell(options))
                using (var deployer = new Deployer(options, localShell, remoteShell))
                {
                    deployer.Execute();
                }
            }
#if DEBUG
            Console.WriteLine("Done!");
#endif
            Exit(0);
        }

        private static void ShowVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            Console.WriteLine(version);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("deploy.exe --init                    creates a new configuration file");
            Console.WriteLine("deploy.exe --version                 displays the product version");
            Console.WriteLine("deploy.exe --help                    displays the available commands");
            Console.WriteLine("deploy.exe --install                 installs the command to the '%Path%'");
            Console.WriteLine("deploy.exe {path-to-config-file}     runs the deployment");
            Console.WriteLine();
            Console.WriteLine("For more information access: http://cartman:3000/marcosbozzani/deploy/README.md");
        }

        public static void Exit(int code)
        {
#if DEBUG
            Console.ReadKey();
#endif

            foreach (var disposable in disposables)
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            Environment.Exit(code);
        }

        public static void Error(string message, params object[] args)
        {
            Console.WriteLine(message, args);
            Exit(1);
        }

        public static void RegisterForDispose(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        private static readonly List<IDisposable> disposables = new List<IDisposable>();
    }
}
