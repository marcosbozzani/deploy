using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deploy
{
    public class PathUtility
    {
        public static string GetFullPath(string path)
        {
            if (path.StartsWith("~"))
            {
                path = "%userprofile%" + path.Substring(1);
            }

            path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                Program.Error("Path not found: '{0}'", path);
            }

            return path;
        }
    }
}
