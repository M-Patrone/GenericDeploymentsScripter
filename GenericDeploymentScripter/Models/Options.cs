using CommandLine;

namespace GenericDeploymentScripter.Models
{
    public class Options
    {
        [Option(
            Required = true,
            HelpText = "Argument to hold the connection string from the dev DB"
            , SetName = "sourceConnectionString")]
        public string sourceConnectionString { get; set; }
        [Option(Required = true,
            HelpText = "Argument to hold the connection string from the target DB")]
        public string targetConnectionString { get; set; }

        [Option(
            Required = true,
            HelpText = "Argument to hold the output path with the filename")]
        public string outputPath { get; set; }

        [Option(
            Required = false,
            HelpText = "Argument to hold the username (with domain)")]
        public string username { get; set; }
    }
}