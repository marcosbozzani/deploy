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
    public static class TarBall
    {
        public static void Create(string sourcePath, string targetPath)
        {
            var previousDirectory = Environment.CurrentDirectory;

            try
            {
                using (var outStream = File.Open(targetPath, FileMode.Create))
                using (var gzoStream = new GZipOutputStream(outStream))
                using (var tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
                {
                    if (Directory.Exists(sourcePath)) // is directory
                    {
                        Environment.CurrentDirectory = sourcePath;
                        tarArchive.AddDirectory(sourcePath);
                    }
                    else
                    {
                        Environment.CurrentDirectory = Path.GetDirectoryName(sourcePath);
                        tarArchive.AddFile(sourcePath);
                    }
                }
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
            }
        }

        public static void AddFile(this TarArchive tarArchive, string path)
        {
            CreateEntry(tarArchive, path);
        }

        public static void AddDirectory(this TarArchive tarArchive, string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                CreateEntry(tarArchive, file);
            }

            foreach (var directory in Directory.GetDirectories(path))
            {
                CreateEntry(tarArchive, directory);
                AddDirectory(tarArchive, directory);
            }
        }

        private static void CreateEntry(TarArchive tarArchive, string file)
        {
            var entry = TarEntry.CreateEntryFromFile(file);

            entry.TarHeader.Mode = entry.IsDirectory ? _dirMode : _fileMode;

            tarArchive.WriteEntry(entry, false);
        }

        private static int GetFileMode(int value)
        {
            return _fileFlag | Convert.ToInt32(Convert.ToString(value), 8);
        }

        private static int GetDirMode(int value)
        {
            return _dirFlag | Convert.ToInt32(Convert.ToString(value), 8);
        }

        private const int _fileMode = _fileFlag | 420; // = 644 octal
        private const int _dirMode = _dirFlag | 493; // = 755 octal

        private const int _dirFlag = 16384; // = 40000 octal
        private const int _fileFlag = 32768; // = 100000 octal
    }
}
