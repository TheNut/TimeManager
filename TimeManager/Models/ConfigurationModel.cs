using System.Collections.Generic;

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

            AzureTableStorageName = string.Empty;
            AzureTableStorageKey = string.Empty;
        }

        public bool TimerLogEntryIncludesComputerDetail { get; set; }
        public bool StateChangeLogEntryIncludesComputerDetail { get; set; }
        public string GitRepositoryPath { get; set; }
        public int PollingIntervalInSeconds { get; set; }

        public string AzureTableStorageName { get; set; }
        public string AzureTableStorageKey { get; set; }
    }
}
