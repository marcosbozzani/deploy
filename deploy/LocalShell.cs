using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deploy
{
    public class LocalShell : IDisposable
    {
        private readonly Process process;

        public LocalShell()
        {
            process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            process.ErrorDataReceived += DataReceived;
            process.OutputDataReceived += DataReceived;
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Console.Write(">> ");
                Console.WriteLine(e.Data);
            }
        }

        public void Execute(string path, int successExitCode = 0)
        {
            if (Path.GetExtension(path) == ".ps1")
            {
                process.StartInfo.FileName = "powershell";
                process.StartInfo.Arguments = "-NoProfile -ExecutionPolicy Unrestricted -File " + path;
            }
            else
            {
                process.StartInfo.FileName = path;
            }

            if (!process.Start())
            {
                Program.Error("Could not start '{0}'", path);
            }

            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            process.WaitForExit();

            if (process.ExitCode != successExitCode)
            {
                Program.Error("The script '{0}' exited with code {1}", path, process.ExitCode);
            }
        }

        public void Dispose()
        {
            process.ErrorDataReceived -= DataReceived;
            process.OutputDataReceived -= DataReceived;

            process.Dispose();
        }
    }
}
