using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GenericDeploymentScripter.Models;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac.Compare;
using Microsoft.SqlServer.Dac.Model;

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
                comparison.Options = new Microsoft.SqlServer.Dac.DacDeployOptions() { AllowIncompatiblePlatform = true, IgnorePermissions = true };
                List<TSqlObject> sourceModel = TSqlModel.LoadFromDatabase(options.sourceConnectionString).GetObjects(DacQueryScopes.All).ToList();
                List<TSqlObject> targetModel = TSqlModel.LoadFromDatabase(options.targetConnectionString).GetObjects(DacQueryScopes.All).ToList();


                var objects = getListOfObjects();


                foreach (TSqlObject excludingObject in sourceModel)
                {
                    string name = String.Join(".", excludingObject.Name.Parts);

                    if (!objects.Contains(name))
                    {
                        comparison.ExcludedSourceObjects.Add(new SchemaComparisonExcludedObjectId(excludingObject.ObjectType, new ObjectIdentifier(excludingObject.Name.Parts.ToArray())));
                        //comparison.ExcludedTargetObjects.Add(new SchemaComparisonExcludedObjectId(excludingObject.ObjectType, new ObjectIdentifier(excludingObject.Name.Parts.ToArray())));

                    }
                }
                /*
                       foreach (TSqlObject excludingObject in targetModel)
                       {
                           string name = String.Join(".", excludingObject.Name.Parts);

                           if (name == "dbo.FAuftragsTarif_20190206")
                           {
                               Console.WriteLine("");
                           }

                           if (!objects.Contains(name))
                           {
                               //comparison.ExcludedSourceObjects.Add(new SchemaComparisonExcludedObjectId(excludingObject.ObjectType, new ObjectIdentifier(excludingObject.Name.Parts.ToArray())));
                               comparison.ExcludedTargetObjects.Add(new SchemaComparisonExcludedObjectId(excludingObject.ObjectType, new ObjectIdentifier(excludingObject.Name.Parts.ToArray())));

                           }
                       }*/

                SchemaComparisonResult result = comparison.Compare();


                //excludeObjects(result);


                // foreach (TSqlObject excludingObject in d)
                // {
                //     if (excludingObject.Name.Parts.Count > 1)
                //     {
                //         if (excludingObject.Name.Parts[1] == "FAuftragsTarif_20190206")
                //         {
                //             Console.Write("test");
                //         }
                //     }
                //     comparison.ExcludedSourceObjects.Add(new SchemaComparisonExcludedObjectId(excludingObject.ObjectType, new ObjectIdentifier(excludingObject.Name.Parts.ToArray())));
                //     comparison.ExcludedTargetObjects.Add(new SchemaComparisonExcludedObjectId(excludingObject.ObjectType, new ObjectIdentifier(excludingObject.Name.Parts.ToArray())));
                // }
                var a = result.GetErrors();
                var differences = result.GenerateScript(target.DatabaseName);
                System.IO.File.WriteAllText(@"D:\Projects\WriteText.txt", differences.Script);

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
            // Parallel.ForEach(results.Differences, diff =>
            // {
            //     Console.WriteLine(a);
            //     TSqlObject b = (diff.SourceObject ?? diff.TargetObject);
            //     var d = b.ObjectType;
            //     var c = b.Name.Parts;

            //     string name = string.Join(".", (diff.SourceObject ?? diff.TargetObject).Name.Parts);
            //     if (!objects.Contains(name))
            //     {
            //         results.Exclude(diff);
            //     }
            //     a++;
            // });

            foreach (var diff in results.Differences)
            {
                Console.WriteLine(a);
                TSqlObject b = (diff.SourceObject ?? diff.TargetObject);
                var d = b.ObjectType;
                var c = b.Name.Parts;

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

                    String sql = "SELECT DISTINCT SchemaName+'.'+ObjectName AS objectName FROM dbo.DDLChanges WHERE LoginName=@id";
                    var cmd = new SqlCommand("SELECT DISTINCT SchemaName+'.'+ObjectName AS objectName FROM dbo.DDLChanges WHERE LoginName=@id");
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