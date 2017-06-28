using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deploy
{
    public class RemoteShell : IDisposable
    {
        private readonly Options options;
        private readonly SshClient ssh;
        private readonly SftpClient sftp;
        private static readonly object executeCommandLock = new object();
        private bool writeLinePrefix;

        public RemoteShell(Options options)
        {
            this.options = options;

            var privateKeyFile = new PrivateKeyFile(PathUtility.GetFullPath(options.PrivateKey));
            var AuthenticationMethod = new PrivateKeyAuthenticationMethod(options.Username, privateKeyFile);
            var connectionInfo = new ConnectionInfo(options.Hostname, options.Port, options.Username, AuthenticationMethod);

            ssh = new SshClient(connectionInfo);
            sftp = new SftpClient(connectionInfo);

            Console.WriteLine("Connecting to {0}@{1}:{2}", options.Username, options.Hostname, options.Port);

            ssh.Connect();
            sftp.Connect();
        }

        public void ExecuteCommand(string command, int successExitCode = 0)
        {
            try
            {
                using (var sshCommand = ssh.CreateCommand(command))
                {
                    sshCommand.OutputDataReceived = DataReceived;
                    sshCommand.ExtendedOutputDataReceived = DataReceived;

                    lock (executeCommandLock)
                    {
                        writeLinePrefix = true;
                        sshCommand.Execute();
                    }

                    if (sshCommand.ExitStatus != successExitCode)
                    {
                        Program.Error("The command '{0}' failed with status {1}", sshCommand.CommandText, sshCommand.ExitStatus);
                    }
                }
            }
            catch (SshException exception)
            {
                Program.Error("SSH error: {0}", exception.Message);
            }
        }

        private void DataReceived(byte[] data, Encoding encoding)
        {
            if (writeLinePrefix)
            {
                Console.Write(">> ");
            }

            var text = encoding.GetString(data);

            writeLinePrefix = text.EndsWith("\n");

            text = text.TrimEnd('\n').Replace("\n", "\n>> ");

            Console.Write(text);

            if (writeLinePrefix)
            {
                Console.Write('\n');
            }
        }

        public void UploadFile(string localPath, string remotePath)
        {
            try
            {
                using (var fileStream = File.OpenRead(localPath))
                {
                    sftp.UploadFile(fileStream, remotePath);
                }
            }
            catch (SshException exception)
            {
                Program.Error("SSH error: {0}\n\tLocal Path: {1}\n\tRemote Path: {2}", exception.Message, localPath, remotePath);
            }
        }

        public void Dispose()
        {
            ssh.Dispose();
            sftp.Dispose();
        }
    }
}
