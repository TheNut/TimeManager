using Microsoft.WindowsAzure.Storage.Table;
using ParentalControl.models;
using System;

namespace TimeManager.Helpers
{
    //"Computer", ComputerID                // The information of unique computers that have program running

    //ComputerID, DateTimeUtc_"Start"       // The information of when the computer starts or stops the service
                                            //  This should probably be the reverse tick format so we can get the top x rows 
                                            //  in reverse entry notation.  (newest first)  without having to sort.

    //"User", UserID                        // The information of the unique user.  The ID is made up of the Domain & Login name
    //UserId, "Computer"_ComputerID         // The list of unique computers the user has used.
    
    //UserId, DateTimeUtc_"Start"





    /// <summary>Information about the Application environment</summary>
    public static class AzureObjects
    {
        /// <summary>Records unique computers that have the program installed.
        /// Entities map to C# objects by using a custom class derived from TableEntity. 
        /// To add an entity to a table, create a class that defines the properties of your entity. 
        /// The following code defines an entity class that uses the customer's first name 
        ///     as the row key and last name as the partition key. 
        /// Together, an entity's partition and row key uniquely identify the entity in the table.
        /// Entities with the same partition key can be queried faster than those with 
        ///     different partition keys, but using diverse partition keys allows for 
        ///     greater scalability of parallel operations. 
        /// For any property that should be stored in the Table service, the property must be 
        ///     a public property of a supported type that exposes both setting and retrieving values. 
        /// Also, your entity type must expose a parameter-less constructor.
        /// </summary>
        public class Computer : TableEntity
        {
            public Computer()
            {
                this.PartitionKey = "ComputerList";
                this.RowKey = InformationModelStatic.MachineInfo.BiosId + ":" + InformationModelStatic.OperatingSystemInfo.NetBiosName;

                this.Info = new InformationModel();

            }

            public InformationModel Info { get; set; }

            public string Email { get; set; }

            public string PhoneNumber { get; set; }
        }



    }
}