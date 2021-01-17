using System;
using System.Collections.Generic;
using CommandLine;
using GenericDeploymentScripter.Logic;
using GenericDeploymentScripter.Models;

namespace GenericDeploymentScripter
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
    .WithParsed(RunOptions)
    .WithNotParsed(HandleParseError);
        }
        static void RunOptions(Options opts)
        {
            Comparer comparer = new Comparer(opts);
            comparer.startCompare();
        }
        static void HandleParseError(IEnumerable<Error> errs)
        {
            Logging.ErrorLogging("Error to init the parameters", errs);
        }
    }
}
