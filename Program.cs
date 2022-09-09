using Azure.Storage.Queues.Models;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace Azure_QueueStorage_Fiddle;

public class Program
{
    private readonly QueueClient _queueClient;
    private const string QueueName = "az204queue";
    private readonly bool _gotQueue;

    public static void Main(string[] args)
    {
        var ctx = new Program();

        if (!ctx._gotQueue)
            return;
        
        ctx.SendMessage();
        ctx.PeekMessage();
        ctx.ChangeContentsOfMessageInQueue(); // Waiting 60 sec in this one!
        ctx.DequeueNextMessage();
        ctx.GetQueueLength();
        ctx.DeleteTheQueue();
    }
    
    private void SendMessage()
    {
        // Insert a message into a queue
        //
        // To insert a message into an existing queue, call the SendMessage method.
        // A message can be either a string (in UTF-8 format) or a byte array. 
        
        var message = $"Yo, {QueueName}!";
        _queueClient.SendMessage(message);
    }
    
    private void PeekMessage()
    {
        // Peek at the next message
        //
        // You can peek at the messages in the queue without removing them from the queue
        // by calling the PeekMessages method. If you don't pass a value for the maxMessages parameter,
        // the default is to peek at one message.
        
        PeekedMessage[] peekedMessage = _queueClient.PeekMessages();
    }
    
    private void ChangeContentsOfMessageInQueue()
    {
        // Change the contents of a queued message
        //
        // You can change the contents of a message in-place in the queue.
        // If the message represents a work task, you could use this feature to
        // update the status of the work task. The following code updates the queue message with new contents,
        // and sets the visibility timeout to extend another 60 seconds.
        // This saves the state of work associated with the message, and gives the client another minute to continue working on the message.
        
        // Get the message from the queue
        QueueMessage[] message = _queueClient.ReceiveMessages();

        // Update the message contents
        _queueClient.UpdateMessage(message[0].MessageId,
            message[0].PopReceipt,
            "Updated contents",
            TimeSpan.FromSeconds(60.0) // Make it invisible for another 60 seconds
        );
        
        var now = DateTime.Now;
        while (DateTime.Now.Subtract(now).Minutes < 1)
        {
            // wait for 60 seconds
        }
        // 60 seconds passed, continue
    }

    private void DequeueNextMessage()
    {
        // De-queue the next message
        // 
        // Dequeue a message from a queue in two steps. When you call ReceiveMessages,
        // you get the next message in a queue. A message returned from ReceiveMessages becomes
        // invisible to any other code reading messages from this queue. By default, this message
        // stays invisible for 30 seconds. To finish removing the message from the queue,
        // you must also call DeleteMessage. This two-step process of removing a message assures
        // that if your code fails to process a message due to hardware or software failure,
        // another instance of your code can get the same message and try again.
        // Your code calls DeleteMessage right after the message has been processed.

        // Get the next message
        QueueMessage[] retrievedMessage = _queueClient.ReceiveMessages();

        // Process (i.e. print) the message in less than 30 seconds
        Console.WriteLine($"Dequeued message: '{retrievedMessage[0].Body}'");

        // Delete the message
        _queueClient.DeleteMessage(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
    }
    
    private void GetQueueLength()
    {
        // Get the queue length
        //
        // You can get an estimate of the number of messages in a queue. The GetProperties method returns
        // queue properties including the message count. The ApproximateMessagesCount property contains
        // the approximate number of messages in the queue. This number is not lower than the actual number
        // of messages in the queue, but could be higher.

        QueueProperties properties = _queueClient.GetProperties();

        // Retrieve the cached approximate message count.
        int cachedMessagesCount = properties.ApproximateMessagesCount;

        // Display number of messages.
        Console.WriteLine($"Number of messages in queue: {cachedMessagesCount}");
    }
    
    private void DeleteTheQueue()
    {
        // Delete a queue
        //
        // To delete a queue and all the messages contained in it, call the Delete method on the queue object.

        // Delete the queue
        _queueClient.Delete();
    }

    private Program()
    {
        // Instantiate a QueueClient which will be used to create and manipulate the queue
        _queueClient = new QueueClient(GetConnectionString(), QueueName);
        
        // Try to create a client for a queue with that name
        try
        {
            // Create the queue (if it doesn't exist)
            _queueClient.CreateIfNotExists();

            if (_queueClient.Exists())
            {
                _gotQueue = true;
                Console.WriteLine($"Queue created or exists: '{_queueClient.Name}'");
            }
            else
            {
                Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}\n\n");
            Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
        }
    }

    private static string GetConnectionString()
    {
        // Build a config object, using env vars and JSON providers.
        var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        // Get values from the config given their key and their target type.
        ConnectionStrings connectionStrings = config.GetRequiredSection("ConnectionStrings").Get<ConnectionStrings>();

        return connectionStrings.StorageAccount.Key1;
    }

    public sealed class ConnectionStrings
    {
        public StorageAccount StorageAccount { get; set; } = null!;
    }

    public sealed class StorageAccount
    {
        public string Key1 { get; set; } = null!;
    }
}