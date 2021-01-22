using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GenericDeploymentScripter.Models;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac.Compare;
using Microsoft.SqlServer.Dac.Model;
using Serilog;
using Serilog.Core;

namespace GenericDeploymentScripter.Logic
{
    public class Comparer
    {

        private readonly Logger _logger;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="options">users parameters</param>
        public Comparer(Options options, Logger log)
        {
            this.options = options;
            this._logger = log;

        }
        private Options options;
        private SchemaCompareDatabaseEndpoint source;
        private SchemaCompareDatabaseEndpoint target;

        private SchemaComparison comparison;

        /// <summary>
        /// method to start the compare of the database
        /// </summary>
        public void startCompare()
        {
            try
            {
                initCompare();
                initSettings();
                SchemaComparisonResult result = comparison.Compare();
                var a = result.GetErrors();
                var differences = result.GenerateScript(target.DatabaseName);

                string preparedScript = getScript(differences.Script);
                System.IO.File.WriteAllText(options.outputPath, preparedScript);
            }
            catch (Exception e)
            {
                _logger.Error("Error in the start method to compare", e);
            }
        }

        /// <summary>
        /// method to init the compare
        /// </summary>
        private void initCompare()
        {
            try
            {
                this.source = new SchemaCompareDatabaseEndpoint(options.sourceConnectionString);
                this.target = new SchemaCompareDatabaseEndpoint(options.targetConnectionString);
                this.comparison = new SchemaComparison(source, target);
            }
            catch (Exception e)
            {
                _logger.Error("Error in the method to init the comparision", e);
            }
        }

        /// <summary>
        /// method to init the comparison settings 
        /// </summary>
        private void initSettings()
        {
            try
            {
                comparison.Options = new Microsoft.SqlServer.Dac.DacDeployOptions() { AllowIncompatiblePlatform = true, IgnorePermissions = true };
            }
            catch (Exception e)
            {
                _logger.Error("Error in the method to set the settings for the comparision", e);
            }

        }
        /// <summary>
        /// get a list of sql objects, which should be included
        /// </summary>
        /// <returns>list of string with sql object names</returns>
        private List<string> getListOfObjects()
        {
            List<string> objectNames = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(options.sourceConnectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand("SELECT DISTINCT SchemaName+'.'+ObjectName AS objectName FROM dbo.DDLChanges WHERE LoginName=@id");
                    cmd.Parameters.Add("@id", SqlDbType.NVarChar).Value = "KULL\\patrone";//options.username;
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
                _logger.Error("SQLException: Error in the method to get the sql objects to be included the comparision", e);
            }
            catch (Exception e)
            {
                _logger.Error("Error in the method to get the sql objects to be included the comparision", e);
            }
            return objectNames;
        }

        /// <summary>
        /// method to set the script together with the sql objects  
        /// </summary>
        /// <param name="strGenScript">generated script from dacfx</param>
        /// <returns>the complete modified sql script</returns>
        private string getScript(string strGenScript)
        {
            try
            {
                List<string> incSQLObjects = getListOfObjects();
                string strIncSQLObjectsScript = "";
                foreach (string incSQLObject in incSQLObjects)
                {

                    strIncSQLObjectsScript += getRequestedSqlObject(strGenScript, incSQLObject.Split(".")[0], incSQLObject.Split(".")[1]);
                    strIncSQLObjectsScript += "\n\n";
                }
                return strIncSQLObjectsScript;
            }
            catch (Exception e)
            {
                _logger.Error("Error in the method to get the sql script to be included the comparision", e);
                return null;
            }
        }

        /// <summary>
        /// Method to compare the generated script with the regex to get only the sql object, which is desired
        /// </summary>
        /// <param name="strToSearch">the generated script from dacfx</param>
        /// <param name="strSchemaName">the sql schema of the sql object</param>
        /// <param name="strObjectName">the name of the sql object</param>
        /// <returns>the object scripted</returns>
        private string getRequestedSqlObject(string strToSearch, string strSchemaName, string strObjectName)
        {
            try
            {
                Regex reg = new Regex(@"(PRINT N\'\[" + strSchemaName + @"\]\.\[" + strObjectName + @"\]) ([^,])+;((.|\n)*?(?=PRINT N\'([^,])+;))");
                return reg.Match(strToSearch).Value;
            }
            catch (Exception e)
            {
                _logger.Error("Error in the method to compare and get the right SQL-Object", e);
                return null;
            }
        }
    }
}