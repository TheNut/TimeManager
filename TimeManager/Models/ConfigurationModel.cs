using System.Collections.Generic;
using TimeManager.Helpers;

namespace TimeManager.Models
{
    public class ConfigurationModel
    {
        public ConfigurationModel()
        {   //Set default values
            TimerLogEntryIncludesComputerDetail = false;
            StateChangeLogEntryIncludesComputerDetail = true;   
            GitRepositoryPath = null;
            PollingIntervalInSeconds = 300;                     //Every 5 minutes = 300 seconds

            //Default to idrive logistics external app storage
            AzureStorageAccountName = "scstorageexternalapp";
            AzureStorageAccountKey = "oIeA9Ck7FvZf5p13BW2Sjzev2YKy5mSLSAFEx308wzeYQ1elc8YO6f5y89X7HHa0veMpFFp3R4fHUoUHEiiRPQ==";
            AzureStorageTableNameComputer = StorageFileManager.AzureTableNameComputer;
            AzureStorageTableNameLog = StorageFileManager.AzureTableNameLog;
        }

        public bool TimerLogEntryIncludesComputerDetail { get; set; }
        public bool StateChangeLogEntryIncludesComputerDetail { get; set; }
        public string GitRepositoryPath { get; set; }
        public int PollingIntervalInSeconds { get; set; }

        public string AzureStorageAccountName { get; set; }
        public string AzureStorageAccountKey { get; set; }
        public string AzureStorageTableNameComputer { get; set; }
        public string AzureStorageTableNameLog { get; set; }
    }
}
