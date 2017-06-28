using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deploy
{
    public class Options
    {
        [JsonProperty(Required = Required.Always)]
        public string Hostname { get; set; }

        [JsonProperty]
        public int Port { get; set; } = 22;

        [JsonProperty(Required = Required.Always)]
        public string Username { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string PrivateKey { get; set; }

        [JsonProperty]
        public string Env { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string SourcePath { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string UploadPath { get; set; }

        [JsonProperty]
        public bool CleanupOnExit { get; set; } = true;

        [JsonProperty]
        public Events LocalShell { get; set; }

        [JsonProperty]
        public Events RemoteShell { get; set; }

        public class Events
        {
            [JsonProperty]
            public string BeforePackage { get; set; }

            [JsonProperty]
            public string AfterPackage { get; set; }

            [JsonProperty]
            public string BeforeUpload { get; set; }

            [JsonProperty]
            public string AfterUpload { get; set; }
        }

        public static Options Load(string[] args)
        {
            var optionsFile = GetOptionsFilePath(args);

            if (!File.Exists(optionsFile))
            {
                Program.Error("Configuration file not found: '{0}'", optionsFile);
            }

            Environment.CurrentDirectory = Path.GetDirectoryName(optionsFile);

            Options options = null;

            try
            {
                options = JsonConvert.DeserializeObject<Options>(File.ReadAllText(optionsFile));
            }
            catch (JsonReaderException exception)
            {
                Program.Error("Parse error: {0} in '{1}'", exception.Message, optionsFile);
            }
            catch (JsonSerializationException exception)
            {
                Program.Error("Parse error: {0} in '{1}'", exception.Message, optionsFile);
            }

            if (!options.UploadPath.EndsWith("/"))
            {
                options.UploadPath += '/';
            }

            Environment.SetEnvironmentVariable("deploy_env", options.Env);

            return options;
        }

        private static string GetOptionsFilePath(string[] args)
        {
            if (args.Length == 0)
            {
                return Path.Combine(Directory.GetCurrentDirectory(), "deploy.json");
            }

            var path = args[0];

            if (!path.EndsWith(".json"))
            {
                path += ".json";
            }

            return PathUtility.GetFullPath(path);
        }
    }
}
