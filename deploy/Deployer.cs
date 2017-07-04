using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deploy
{
    public class Deployer : IDisposable
    {
        private const string packageName = "package.tar.gz";

        private readonly Options options;
        private readonly LocalShell localShell;
        private readonly RemoteShell remoteShell;

        private bool disposed = false;
        private string localTempDir = null;
        private string remoteTempDir = null;

        public Deployer(Options options, LocalShell localShell, RemoteShell remoteShell)
        {
            this.options = options;
            this.localShell = localShell;
            this.remoteShell = remoteShell;

            Program.RegisterForDispose(this);
        }

        public void Execute()
        {
            Initialize();
            Package();
            Upload();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            if (options.CleanupOnExit)
            {
                Console.WriteLine("Cleaning up...");

                TryRemoveLocalTempDir(localTempDir);
                TryRemoveRemoteTempDir(remoteTempDir);
            }
        }

        private void Initialize()
        {
            var version = "deploy_" + Path.GetRandomFileName();

            localTempDir = CreateLocalTempDir(version);
            remoteTempDir = CreateRemoteTempDir(version);
        }

        private void Package()
        {
            RaiseBeforePackage();

            var sourcePath = PathUtility.GetFullPath(options.SourcePath);
            var targetPath = Path.Combine(localTempDir, packageName);

            Console.WriteLine("Packaging:");
            Console.WriteLine(">> Source: {0}", sourcePath);
            Console.WriteLine(">> Target: {0}", targetPath);

            TarBall.Create(sourcePath, targetPath);

            RaiseAfterPackage();
        }

        private void Upload()
        {
            RaiseBeforeUpload();

            var localPath = Path.Combine(localTempDir, packageName);
            var remotePath = remoteTempDir + packageName;

            Console.WriteLine("Uploading:");
            Console.WriteLine(">> Source: {0}", localPath);
            Console.WriteLine(">> Target: {0}", remotePath);

            remoteShell.UploadFile(localPath, remotePath);

            RaiseAfterUpload();
        }

        private string CreateLocalTempDir(string version)
        {
            var localTempDir = Path.Combine(Path.GetTempPath(), version);

            Directory.CreateDirectory(localTempDir);

            return localTempDir;
        }

        private string CreateRemoteTempDir(string version)
        {
            var remotePath = options.UploadPath + version + "/";

            remoteShell.ExecuteCommand("mkdir -p " + remotePath);

            return remotePath;
        }

        private void TryRemoveLocalTempDir(string path)
        {
            try
            {
                if (path != null && Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch { }
        }

        private void TryRemoveRemoteTempDir(string path)
        {
            try
            {
                if (path != null)
                {
                    remoteShell.ExecuteCommand("sudo rm -rf " + path);
                }
            }
            catch { }
        }

        private void RaiseBeforePackage()
        {
            if (options.LocalShell != null && !string.IsNullOrEmpty(options.LocalShell.BeforePackage))
            {
                LocalExecute(options.LocalShell.BeforePackage);
            }

            if (options.RemoteShell != null && !string.IsNullOrEmpty(options.RemoteShell.BeforePackage))
            {
                RemoteExecute(options.RemoteShell.BeforePackage);
            }
        }

        private void RaiseAfterPackage()
        {
            if (options.LocalShell != null && !string.IsNullOrEmpty(options.LocalShell.AfterPackage))
            {
                LocalExecute(options.LocalShell.AfterPackage);
            }

            if (options.RemoteShell != null && !string.IsNullOrEmpty(options.RemoteShell.AfterPackage))
            {
                RemoteExecute(options.RemoteShell.AfterPackage);
            }
        }

        private void RaiseBeforeUpload()
        {
            if (options.LocalShell != null && !string.IsNullOrEmpty(options.LocalShell.BeforeUpload))
            {
                LocalExecute(options.LocalShell.BeforeUpload);
            }

            if (options.RemoteShell != null && !string.IsNullOrEmpty(options.RemoteShell.BeforeUpload))
            {
                RemoteExecute(options.RemoteShell.BeforeUpload);
            }
        }

        private void RaiseAfterUpload()
        {
            if (options.LocalShell != null && !string.IsNullOrEmpty(options.LocalShell.AfterUpload))
            {
                LocalExecute(options.LocalShell.AfterUpload);
            }

            if (options.RemoteShell != null && !string.IsNullOrEmpty(options.RemoteShell.AfterUpload))
            {
                RemoteExecute(options.RemoteShell.AfterUpload);
            }
        }

        private void LocalExecute(string path)
        {
            path = PathUtility.GetFullPath(path);

            Console.WriteLine("Executing: {0}", path);

            localShell.Execute(path);
        }

        private void RemoteExecute(string localPath)
        {
            localPath = PathUtility.GetFullPath(localPath);

            Console.WriteLine("Executing: {0}", localPath);

            var remotePath = remoteTempDir + string.Format("script_{0}.sh", Path.GetRandomFileName());

            remoteShell.UploadFile(localPath, remotePath);

            remoteShell.ExecuteCommand("chmod +x " + remotePath);

            var command = string.Format("cd {0}; export DEPLOY_ENV=\"{1}\"; {2}", remoteTempDir, options.Env, remotePath);

            remoteShell.ExecuteCommand(command);

            remoteShell.ExecuteCommand("rm -f " + remotePath);
        }
    }
}
