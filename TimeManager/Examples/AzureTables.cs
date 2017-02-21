using Microsoft.Azure;                      // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage;       // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types
using System;

namespace TimeManager.Examples
{
    class AzureTablesExample
    {
        /*
            https://docs.microsoft.com/en-us/azure/storage/storage-dotnet-how-to-use-tables
         */

        /// <summary>Does examples of basic actions on azure tables</summary>
        /// <param name="ConfigurationConnectionId">The name of the connection definition</param>
        public static void ExampleOfAzureTables(string ConfigurationConnectionId = "example")
        {
            #region Create a table

            /*
                Entities map to C# objects by using a custom class derived from TableEntity. 
                To add an entity to a table, create a class that defines the properties of your entity. 
                The following code defines an entity class that uses the customer's first name as the 
                    row key and last name as the partition key. 
                Together, an entity's partition and row key uniquely identify the entity in the table. 
                Entities with the same partition key can be queried faster than those with different 
                    partition keys, but using diverse partition keys allows for greater scalability 
                    of parallel operations. 
                For any property that should be stored in the Table service, the property must be a 
                    public property of a supported type that exposes both setting and retrieving values. 
                Also, your entity type must expose a parameter-less constructor.
             */

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(ConfigurationConnectionId));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Retrieve a reference to the table.
            CloudTable table = tableClient.GetTableReference("people");

            // Create the table if it doesn't exist.
            table.CreateIfNotExists();

            #endregion

            #region Add a single entity to a table
            /*
             Table operations that involve entities are performed via the CloudTable object 
             that you created earlier in the "Create a table" section. The operation to be 
             performed is represented by a TableOperation object. The following code example 
             shows the creation of the CloudTable object and then a CustomerEntity object. 
             To prepare the operation, a TableOperation object is created to insert the 
             customer entity into the table. Finally, the operation is executed by calling 
             CloudTable.Execute.
             */
            // Create a new customer entity.
            CustomerEntity customer0 = new CustomerEntity("Harp", "Walter");
            customer0.Email = "Walter@contoso.com";
            customer0.PhoneNumber = "425-555-0101";

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.Insert(customer0);

            // Execute the insert operation.
            table.Execute(insertOperation);

            #endregion

            #region Insert a batch of entities to a table
            /*
                You can insert a batch of entities into a table in one write operation. Some other notes on batch operations:

                    * You can perform updates, deletes, and inserts in the same single batch operation.
                    * A single batch operation can include up to 100 entities.
                    * All entities in a single batch operation must have the same partition key.
                    * While it is possible to perform a query as a batch operation, it must be the only operation in the batch.

                The following code example creates two entity objects and adds each to TableBatchOperation by using the Insert method. 
                Then, CloudTable.Execute is called to execute the operation.
             */

            // Create the batch operation.
            TableBatchOperation batchOperation = new TableBatchOperation();

            // Create a customer entity and add it to the table.
            CustomerEntity customer1 = new CustomerEntity("Smith", "Jeff");
            customer1.Email = "Jeff@contoso.com";
            customer1.PhoneNumber = "425-555-0104";

            // Create another customer entity and add it to the table.
            CustomerEntity customer2 = new CustomerEntity("Smith", "Ben");
            customer2.Email = "Ben@contoso.com";
            customer2.PhoneNumber = "425-555-0102";

            // Add both customer entities to the batch insert operation.
            batchOperation.Insert(customer1);
            batchOperation.Insert(customer2);

            // Execute the batch operation.
            table.ExecuteBatch(batchOperation);

            #endregion

            #region Retrieve all entities in a partition

            /*
              To query a table for all entities in a partition, use a TableQuery object. 
              The following code example specifies a filter for entities where 'Smith' is the partition key. 
              This example prints the fields of each entity in the query results to the console.
             */

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<CustomerEntity> query = new TableQuery<CustomerEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Smith"));

            // Print the fields for each customer.
            foreach (CustomerEntity entity in table.ExecuteQuery(query))
            {
                Console.WriteLine("{0}, {1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey,
                    entity.Email, entity.PhoneNumber);
            }

            #endregion

            #region Retrieve a range of of entities in partition

            /*
                If you don't want to query all the entities in a partition, you can specify a 
                    range by combining the partition key filter with a row key filter. 
                The following code example uses two filters to get all entities in partition 
                    'Smith' where the row key (first name) starts with a letter earlier than 
                    'E' in the alphabet and then prints the query results.
             */

            // Create the table query.
            TableQuery<CustomerEntity> rangeQuery = new TableQuery<CustomerEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Smith"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, "E")));

            // Loop through the results, displaying information about the entity.
            foreach (CustomerEntity entity in table.ExecuteQuery(rangeQuery))
            {
                Console.WriteLine("{0}, {1}\t{2}\t{3}", entity.PartitionKey, entity.RowKey,
                    entity.Email, entity.PhoneNumber);
            }

            #endregion

            #region Retrieve a single entity

            /*
                You can write a query to retrieve a single, specific entity. 
                The following code uses TableOperation to specify the customer 'Ben Smith'. 
                This method returns just one entity rather than a collection, and the 
                    returned value in TableResult.Result is a CustomerEntity object. 
                Specifying both partition and row keys in a query is the fastest way to 
                    retrieve a single entity from the Table service.
             */

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<CustomerEntity>("Smith", "Ben");

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Print the phone number of the result.
            if (retrievedResult.Result != null)
                Console.WriteLine(((CustomerEntity)retrievedResult.Result).PhoneNumber);
            else
                Console.WriteLine("The phone number could not be retrieved.");

            #endregion

            #region Replace an entity

            /*
                To update an entity, retrieve it from the Table service, modify the entity object, 
                    and then save the changes back to the Table service. 
                The following code changes an existing customer's phone number. 
                Instead of calling Insert, this code uses Replace. 
                This causes the entity to be fully replaced on the server, unless the entity on the 
                    server has changed since it was retrieved, in which case the operation will fail. 
                This failure is to prevent your application from inadvertently overwriting a change 
                    made between the retrieval and update by another component of your application. 
                The proper handling of this failure is to retrieve the entity again, make your changes 
                    (if still valid), and then perform another Replace operation. 
                The next section will show you how to override this behavior.
             */

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation2 = TableOperation.Retrieve<CustomerEntity>("Smith", "Ben");

            // Execute the operation.
            TableResult retrievedResult2 = table.Execute(retrieveOperation2);

            // Assign the result to a CustomerEntity object.
            CustomerEntity updateEntity = (CustomerEntity)retrievedResult2.Result;

            if (updateEntity != null)
            {
                // Change the phone number.
                updateEntity.PhoneNumber = "425-555-0105";

                // Create the Replace TableOperation.
                TableOperation updateOperation = TableOperation.Replace(updateEntity);

                // Execute the operation.
                table.Execute(updateOperation);

                Console.WriteLine("Entity updated.");
            }
            else
                Console.WriteLine("Entity could not be retrieved.");

            #endregion

            #region Insert-or-replace an entity

            /*
                Replace operations will fail if the entity has been changed since it was 
                    retrieved from the server. 
                Furthermore, you must retrieve the entity from the server first in order 
                    for the Replace operation to be successful. 
                Sometimes, however, you don't know if the entity exists on the server and 
                    the current values stored in it are irrelevant. 
                Your update should overwrite them all. 
                To accomplish this, you would use an InsertOrReplace operation. 
                This operation inserts the entity if it doesn't exist, or replaces it if 
                    it does, regardless of when the last update was made. 
                In the following code example, the customer entity for Ben Smith is still 
                    retrieved, but it is then saved back to the server via InsertOrReplace. 
                Any updates made to the entity between the retrieval and update operations 
                    will be overwritten.
             */

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation3 = TableOperation.Retrieve<CustomerEntity>("Smith", "Ben");

            // Execute the operation.
            TableResult retrievedResult3 = table.Execute(retrieveOperation3);

            // Assign the result to a CustomerEntity object.
            CustomerEntity updateEntity3 = (CustomerEntity)retrievedResult3.Result;

            if (updateEntity3 != null)
            {
                // Change the phone number.
                updateEntity3.PhoneNumber = "425-555-1234";

                // Create the InsertOrReplace TableOperation.
                TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(updateEntity3);

                // Execute the operation.
                table.Execute(insertOrReplaceOperation);

                Console.WriteLine("Entity was updated.");
            }

            else
                Console.WriteLine("Entity could not be retrieved.");

            #endregion

            #region Query a subset of entity properties

            /*
                A table query can retrieve just a few properties from an entity instead of 
                    all the entity properties. 
                This technique, called projection, reduces bandwidth and can improve query 
                    performance, especially for large entities. 
                The query in the following code returns only the email addresses of entities 
                    in the table. 
                This is done by using a query of DynamicTableEntity and also EntityResolver. 
                You can learn more about projection on the Introducing Upsert and Query 
                    Projection blog post. 
                Note that projection is not supported on the local storage emulator, so this 
                    code runs only when you're using an account on the Table service.
             */

            // Define the query, and select only the Email property.
            TableQuery<DynamicTableEntity> projectionQuery = new TableQuery<DynamicTableEntity>().Select(new string[] { "Email" });

            // Define an entity resolver to work with the entity after retrieval.
            EntityResolver<string> resolver = (pk, rk, ts, props, etag) => props.ContainsKey("Email") ? props["Email"].StringValue : null;

            foreach (string projectedEmail in table.ExecuteQuery(projectionQuery, resolver, null, null))
            {
                Console.WriteLine(projectedEmail);
            }

            #endregion

            #region Delete an entity
            /*
                You can easily delete an entity after you have retrieved it, by using 
                    the same pattern shown for updating an entity. 
                The following code retrieves and deletes a customer entity.
             */

            // Create a retrieve operation that expects a customer entity.
            TableOperation retrieveOperation4 = TableOperation.Retrieve<CustomerEntity>("Smith", "Ben");

            // Execute the operation.
            TableResult retrievedResult4 = table.Execute(retrieveOperation4);

            // Assign the result to a CustomerEntity.
            CustomerEntity deleteEntity4 = (CustomerEntity)retrievedResult4.Result;

            // Create the Delete TableOperation.
            if (deleteEntity4 != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity4);

                // Execute the operation.
                table.Execute(deleteOperation);

                Console.WriteLine("Entity deleted.");
            }
            else
                Console.WriteLine("Could not retrieve the entity.");

            #endregion

            #region Retrieve entities in pages asynchronously

            /*
                If you are reading a large number of entities, and you want to process/display 
                    entities as they are retrieved rather than waiting for them all to return, 
                    you can retrieve entities by using a segmented query. 
                This example shows how to return results in pages by using the Async-Await 
                    pattern so that execution is not blocked while you're waiting for a large 
                    set of results to return. 
                For more details on using the Async-Await pattern in .NET, 
                    see Asynchronous programming with Async and Await (C# and Visual Basic).
             */

            // Initialize a default TableQuery to retrieve all the entities in the table.
            TableQuery<CustomerEntity> tableQuery = new TableQuery<CustomerEntity>();

            // Initialize the continuation token to null to start from the beginning of the table.
            TableContinuationToken continuationToken = null;

            do
            {
                // Retrieve a segment (up to 100 entities).
                TableQuerySegment<CustomerEntity> tableQueryResult =
                    //await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
                    table.ExecuteQuerySegmented(tableQuery, continuationToken);

                // Assign the new continuation token to tell the service where to
                // continue on the next iteration (or null if it has reached the end).
                continuationToken = tableQueryResult.ContinuationToken;

                // Print the number of rows retrieved.
                Console.WriteLine("Rows retrieved {0}", tableQueryResult.Results.Count);

                // Loop until a null continuation token is received, indicating the end of the table.
            } while (continuationToken != null);

            #endregion

            #region Delete a table

            /*
                Finally, the following code example deletes a table from a storage account. 
                A table that has been deleted will be unavailable to be re-created for a 
                    period of time following the deletion.
             */

            // Print the number of rows retrieved.
            Console.WriteLine("Removing table {0}, {1}", table.Name, table.Uri);

            // Delete the table it if exists.
            table.DeleteIfExists();

            #endregion
        }

        /// <summary>
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
        public class CustomerEntity : TableEntity
        {
            public CustomerEntity(string lastName, string firstName)
            {
                this.PartitionKey = lastName;
                this.RowKey = firstName;
            }

            public CustomerEntity() { }

            public string Email { get; set; }

            public string PhoneNumber { get; set; }
        }
        
    }
}
