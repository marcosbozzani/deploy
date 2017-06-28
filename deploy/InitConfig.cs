using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Deploy
{
    public class InitConfig
    {
        public static void Create()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var inputName = "Deploy.Init.json";
            var outputName = Path.GetFullPath("deploy.json");

            if (File.Exists(outputName))
            {
                Program.Error("The file '{0}' already exists", outputName);
            }

            using (var stream = assembly.GetManifestResourceStream(inputName))
            using (var file = File.Create(outputName))
            {
                stream.CopyTo(file);
            }

            Console.WriteLine("The file '{0}' was created", outputName);
        }
    }
}
