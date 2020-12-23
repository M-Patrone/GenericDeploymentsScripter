using System;
using System.Collections.Generic;
using System.Data;
using GenericDeploymentScripter.Models;
using Microsoft.Data.SqlClient;
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
                //comparison.SaveToFile("testfile.txt", true);
                comparison.Options = new Microsoft.SqlServer.Dac.DacDeployOptions() { AllowIncompatiblePlatform = true };
                SchemaComparisonResult result = comparison.Compare();
                var a = result.GetErrors();
                excludeObjects(result);
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
            List<string> objects = getListOfObjects();
            int a = 0;
            foreach (SchemaDifference diff in results.Differences)
            {
                Console.WriteLine(a);
                string name = string.Join(".", (diff.SourceObject ?? diff.TargetObject).Name.Parts);
                if (!objects.Contains(name))
                {
                    results.Exclude(diff);
                }
                a++;
            }
            Console.WriteLine("dd");
        }

        private List<string> getListOfObjects()
        {
            List<string> objectNames = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(options.sourceConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    connection.Open();

                    String sql = "SELECT SchemaName+'.'+ObjectName AS objectName FROM dbo.DDLChanges WHERE LoginName=@id";
                    var cmd = new SqlCommand("SELECT SchemaName+'.'+ObjectName AS objectName FROM dbo.DDLChanges WHERE LoginName=@id");
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = options.username;
                    cmd.Connection = connection;

                    using (cmd)
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine("{0}", reader.GetString(0));
                                objectNames.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return objectNames;
        }

    }
}