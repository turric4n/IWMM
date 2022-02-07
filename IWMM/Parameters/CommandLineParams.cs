using CommandLine;

namespace IWMM.Parameters
{
    public class CommandLineParams
    {
        [Option('c', "config", Required = false, HelpText = "Set configuration file path")]
        public string ConfigFile { get; set; } = "iwmm.yml";

        public bool WebHost { get; set; } = false;
    }
}
