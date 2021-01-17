namespace GenericDeploymentScripter.Logic
{
    public class Logging
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// method to log info messages
        /// </summary>
        /// <param name="strMessage">specific message from dev</param>
        /// <param name="oMessage">objects to log</param>
        public static void InfoLogging(string strMessage, object oMessage)
        {
            logger.Info(strMessage, oMessage);
        }
        /// <summary>
        /// method to log warn messages
        /// </summary>
        /// <param name="strMessage">specific message from dev</param>
        /// <param name="oMessage">objects to log</param>
        public static void WarnLogging(string strMessage, object oMessage)
        {
            logger.Warn(strMessage, oMessage);
        }
        /// <summary>
        /// method to log error messages
        /// </summary>
        /// <param name="strMessage">specific message from dev</param>
        /// <param name="oMessage">error message to log</param>
        public static void ErrorLogging(string strMessage, object oMessage)
        {
            logger.Error(strMessage, oMessage);
        }
    }
}