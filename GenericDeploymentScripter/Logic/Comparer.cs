using System;
using System.Collections.Generic;
using GenericDeploymentScripter.Models;
using Microsoft.SqlServer.Dac.Compare;

namespace GenericDeploymentScripter.Logic
{
    public class Comparer
    {
        public Options options { get; set; }

        public void startCompare()
        {
            try
            {
                SchemaCompareDatabaseEndpoint source = new SchemaCompareDatabaseEndpoint(options.sourceConnectionString);
                SchemaCompareDatabaseEndpoint target = new SchemaCompareDatabaseEndpoint(options.targetConnectionString);

                var comparison = new SchemaComparison(source, target);
                comparison.SaveToFile("testfile.txt", true);
                comparison.Options = new Microsoft.SqlServer.Dac.DacDeployOptions() { AllowIncompatiblePlatform = true };

                SchemaComparisonResult result = comparison.Compare();
                var a = result.GetErrors();
                //excludeObjects(result);
                var differences = result.GenerateScript(target.DatabaseName);
                string script = differences.Script;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        private void excludeObjects(SchemaComparisonResult results)
        {
            foreach (SchemaDifference diff in results.Differences)
            {
                string name = string.Join(".", (diff.SourceObject ?? diff.TargetObject).Name.Parts);
                // if (postExcludeProp.Contains(name))
                //{
                results.Exclude(diff);
                // }
            }
        }

        //private void getListOf
    }

}