using Microsoft.Azure;                      // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage;       // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage typesusing Newtonsoft.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using TimeManager.Models;

namespace TimeManager.Helpers
{
    /// <summary>Handle the reading and writing of information to the storage file</summary>
    public static class StorageFileManager
    {
        #region Private Variables

        /// <summary>The defined storage locations</summary>
        private enum StorageLocation
        {
            TempFolder = -1,                                                // non environment folder
            Desktop = Environment.SpecialFolder.Desktop,                    // 0
            MyDocuments = Environment.SpecialFolder.MyDocuments,            // 5
            MyPictures = Environment.SpecialFolder.MyPictures,              //39
            AppDataRoaming = Environment.SpecialFolder.ApplicationData,     //26
            AppDataLocal = Environment.SpecialFolder.LocalApplicationData   //28

        }
        private static StorageLocation _configLocationType = StorageLocation.MyDocuments;
        private static string _configFileName = "TimeTrackerConfig.txt";
        private static StorageLocation _storageLocationType = StorageLocation.TempFolder;
        private static string _storageFileName = "TimeTracker.txt";
        private static string _azureTableComputer = "TimeTrackerComputer";
        private static string _azureTableLog = "TimeTrackerLog";

        #endregion

        #region Methods

        /// <summary>Write out the Config to a file</summary>
        /// <param name="config">string list of config entries</param>
        public static void WriteConfig(ConfigurationModel config)
        {
            try
            {
                File.WriteAllText(GetConfigPathAndFileName(), JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            catch (Exception)
            {
                //supress any config entry writing exception
            }
        }
        /// <summary>Read in the entire configfile</summary>
        /// <returns></returns>
        public static ConfigurationModel ReadConfig()
        {
            try
            {
                if (!File.Exists(GetConfigPathAndFileName()))
                    WriteConfig(new ConfigurationModel());

                return JsonConvert.DeserializeObject<ConfigurationModel>(File.ReadAllText(GetConfigPathAndFileName()), new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error });
            }
            catch (Exception)
            {
                ConfigurationModel temp = new ConfigurationModel();
                try
                {
                    //rewrite the config file, preserving everything i can
                    temp = JsonConvert.DeserializeObject<ConfigurationModel>(File.ReadAllText(GetConfigPathAndFileName()), new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });
                    WriteConfig(temp);
                }
                catch (Exception)
                {
                    WriteConfig(new ConfigurationModel());
                }
                //Return the fixed config file
                return temp;
            }
        }

        /// <summary>Log an entry into the log file for the logged in user.
        ///     User login name will be prepended to the log entry
        /// </summary>
        /// <param name="logEntry">String of the log entry</param>
        public static void Write(string logEntry) { Write(new List<string>() { logEntry }, GetLoginUserName()); }
        /// <summary>Log an entry into the log file for a given userLoginName.
        ///     User login name will be prepended to the log entry
        /// </summary>
        /// <param name="logEntry">String of the log entry</param>
        /// <param name="userLoginName">The login name of the user</param>
        public static void Write(string logEntry, string userLoginName) { Write(new List<string>() { logEntry }, userLoginName); }

        /// <summary>Log many entries into the log file for the logged in user.
        ///     User login name will be prepended to each log entry
        /// </summary>
        /// <param name="logEntries">string list of log entries</param>
        public static void Write(List<string> logEntries) { Write(logEntries, GetLoginUserName()); }
        /// <summary>Log many entries into the log file for the specified userLoginName.
        ///     User login name will be prepended to each log entry
        /// </summary>
        /// <param name="logEntries">string list of log entries</param>
        /// <param name="userLoginName">the user login name</param>
        public static void Write(List<string> logEntries, string userLoginName)
        {
            try
            {   //The template of the entry in the log file
                string entryTemplate = "{0} - ({1:yyyy-MM-dd HH:mm:ss}) - {2}";

                //Prepend the userLoginName to each log line
                logEntries = logEntries.Select(entry => string.Format(entryTemplate, userLoginName, DateTime.Now, entry)).ToList();
                //Append the log lines to the file
                File.AppendAllLines(GetPathAndFileName(), logEntries);
            }
            catch (Exception)
            {
                //supress any log entry writing exception
            }
        }

        /// <summary>Clear out the entire log file</summary>
        public static void Clear() { File.WriteAllText(GetPathAndFileName(), string.Empty); }
        /// <summary>Clear out the entire log for the specified userLoginName</summary>
        /// <param name="userLoginName">The userLoginName to remove</param>
        public static void Clear(string userLoginName)
        {
            try
            {
                //Write the log without the specified userLoginName
                File.WriteAllLines(GetPathAndFileName(), File.ReadAllLines(GetPathAndFileName()).Where(l => !l.StartsWith(userLoginName + " - ")).ToList());
            }
            catch (Exception)
            {
                //supress any log entry writing exception
            }
        }

        /// <summary>Read in the entire log file</summary>
        /// <returns></returns>
        public static List<string> Read() { return Read(string.Empty); }
        /// <summary>Read in the log file lines from the given user</summary>
        /// <param name="userLoginName">The username</param>
        /// <returns></returns>
        public static List<string> Read(string userLoginName)
        {
            try
            {
                //See if we have no name defined, if not, then get last entry in log
                if (string.IsNullOrWhiteSpace(userLoginName))
                    return File.ReadAllLines(GetPathAndFileName()).ToList();

                //Read all lines for the given userLoginName
                return File.ReadAllLines(GetPathAndFileName()).Where(l => l.StartsWith(userLoginName + " - ")).ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        /// <summary>Get the last log entry of the log file</summary>
        /// <returns></returns>
        public static string ReadLastEntry() { return ReadLastEntry(string.Empty); }
        /// <summary>Get the last log entry for the specified userLoginName. Leave empty for last entry irregardless of user that produced it.</summary>
        /// <param name="userLoginName">the userLoginName information to retrieve</param>
        /// <returns></returns>
        public static string ReadLastEntry(string userLoginName)
        {
            try
            {
                //See if we have no name defined, if not, then get last entry in log
                if (string.IsNullOrWhiteSpace(userLoginName))
                    return File.ReadAllLines(GetPathAndFileName()).Last();

                //return the last entry for the given name
                return File.ReadAllLines(GetPathAndFileName()).Where(l => l.StartsWith(userLoginName + " - ")).Last();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #endregion

        #region Helper Methods

        public static void AzureComputerLog()
        {
            ConfigurationModel config = ReadConfig();
            string connectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                                                    config.AzureTableStorageName, 
                                                    config.AzureTableStorageKey);
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(connectionString));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("people");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

        }

        /// <summary>Get the current login user</summary>
        public static string GetLoginUserName()
        {
            try
            {
                return WindowsIdentity.GetCurrent().Name.ToString();
            }
            catch (System.Security.SecurityException ex)
            {
                return "UnknownUserForSecurityException";
            }
            catch (Exception)
            {
                return "UnknownUserUnknownException";
            }
        }
        /// <summary>Get the storage file name</summary>
        /// <returns></returns>
        public static string GetFileName()
        {
            if (string.IsNullOrWhiteSpace(_storageFileName))
                _storageFileName = Path.GetFileName(Path.GetTempFileName());

            return _storageFileName;
        }
        /// <summary>Get the path and filename of the log file</summary>
        /// <returns></returns>
        public static string GetPath()
        {
            //Set the location folder
            switch (_storageLocationType)
            {
                case StorageLocation.TempFolder: return Path.GetTempPath();
                case StorageLocation.Desktop: return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                case StorageLocation.MyDocuments: return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                case StorageLocation.AppDataRoaming: return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                case StorageLocation.AppDataLocal: return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                default: return Path.GetTempPath();
            }
        }
        /// <summary>Get the path and filename of the log file</summary>
        /// <returns></returns>
        public static string GetPathAndFileName() { return Path.Combine(GetPath(), GetFileName()); }

        /// <summary>Get the name of the config file</summary>
        /// <returns></returns>
        public static string GetConfigFileName()
        {
            if (string.IsNullOrWhiteSpace(_configFileName))
                _configFileName = Path.GetFileName(Path.GetTempFileName());

            return _configFileName;
        }
        /// <summary>Get the path of the config file</summary>
        /// <returns></returns>
        public static string GetConfigPath()
        {
            //Set the location folder
            switch (_configLocationType)
            {
                case StorageLocation.TempFolder: return Path.GetTempPath();
                case StorageLocation.Desktop: return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                case StorageLocation.MyDocuments: return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                case StorageLocation.AppDataRoaming: return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                case StorageLocation.AppDataLocal: return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                default: return Path.GetTempPath();
            }
        }
        /// <summary>Get the path and filename of the config</summary>
        /// <returns></returns>
        public static string GetConfigPathAndFileName() { return Path.Combine(GetConfigPath(), GetConfigFileName()); }

        #endregion Helper Methods
    }
}
