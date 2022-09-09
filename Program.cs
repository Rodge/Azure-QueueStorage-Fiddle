namespace Azure_QueueStorage_Fiddle;

using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

public class Program
{
    private readonly QueueClient? _queueClient;
    
    public static async Task Main(string[] args)
    {
        Program ctx = new Program();

        if (ctx._queueClient == null)
        {
            
        }
        
        // Bla bla
    }
    
    private Program()
    {
        const string queueName = "az204queue";
        var connectionString = GetConnectionString();
        
        // Try to create a client for a queue with that name
        try
        {
            // Instantiate a QueueClient which will be used to create and manipulate the queue
            _queueClient = new QueueClient(connectionString, queueName);

            // Create the queue (if it doesn't exist)
            _queueClient.CreateIfNotExists();

            Console.WriteLine(_queueClient.Exists()
                ? $"Queue created or exists: '{_queueClient.Name}'"
                : $"Make sure the Azurite storage emulator running and try again.");
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