using Client;
using Common.Encryption;
using Common.Padding;
using Common.Protocols;
using System.Net;
using System.Text;

#region Configuration
var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8888);
int receivingBufferSize = 1024;
var protocol = new SimpleSessionLayerProtocol(4);
var encryptionKey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
var bitPaddingProvider = new PkcsBitPaddingProvider(TeaCipher.DataBlockSize);
var cipher = new TeaCipher(encryptionKey, bitPaddingProvider);
#endregion

#region Methods
async Task MonitorConnection(ClientSocket clientSocket, CancellationToken cancellationToken)
{
    #region Arguments validation
    if (clientSocket is null)
    {
        string argumentName = nameof(clientSocket);
        const string ErrorMessage = "Provided client socket a null reference:";
        throw new ArgumentNullException(argumentName, ErrorMessage);
    }
    #endregion

    bool lastConnectionState = false;
    bool currentConnectionState = false;

    while (!cancellationToken.IsCancellationRequested)
    {
        lastConnectionState = currentConnectionState;
        currentConnectionState = clientSocket.IsConnectionEstablished;

        if (lastConnectionState)
        {
            if (!currentConnectionState)
            {
                Console.WriteLine("Connection to server closed.");
                Console.WriteLine("Press ENTER to shutdown the client.");
            }
        }
        else
        {
            if (currentConnectionState)
            {
                Console.WriteLine("Connection to server established.");
            }
        }

        await Task.Delay(200);
    }
}

async Task NotifyAboutIncomingMessages(ClientSocket clientSocket, CancellationToken cancellationToken)
{
    #region Arguments validation
    if (clientSocket is null)
    {
        string argumentName = nameof(clientSocket);
        const string ErrorMessage = "Provided client socket a null reference:";
        throw new ArgumentNullException(argumentName, ErrorMessage);
    }
    #endregion

    var binaryContent = Array.Empty<byte>();

    while (!cancellationToken.IsCancellationRequested)
    {
        binaryContent = clientSocket.GetReceivedData();

        if (binaryContent is not null)
        {
            string textContent = Encoding.UTF8.GetString(binaryContent);
            Console.WriteLine($"New message from server: {textContent}");
        }

        await Task.Delay(200);
    }
}
#endregion

#region Main
var tasks = new List<Task>();
var cancelationTokenSource = new CancellationTokenSource();

Console.WriteLine("Starting the client...");
using (var client = new ClientSocket(serverEndPoint, receivingBufferSize, protocol, cipher))
{
    Task connectionMonitoringTask = MonitorConnection(client, cancelationTokenSource.Token);
    tasks.Add(connectionMonitoringTask);

    Task messageNotificationTask = NotifyAboutIncomingMessages(client, cancelationTokenSource.Token);
    tasks.Add(messageNotificationTask);

    Console.WriteLine("Connecting to server...");
    client.ConnectToServer();
    while (!client.IsConnectionEstablished)
    {
        Thread.Sleep(200);
    }

    while (client.IsConnectionEstablished)
    {
        string? userInput = Console.ReadLine();

        if (userInput is not null)
        {
            if (userInput == "end")
            {
                break;
            }

            byte[] inputAsBytes = Encoding.UTF8.GetBytes(userInput);
            client.SentData(inputAsBytes);
        }
    }

    Console.WriteLine("Shutting down the client...");
    cancelationTokenSource.Cancel();
    Task.WaitAll(tasks.ToArray());
}

Console.WriteLine("Press ENTER to continue...");
Console.ReadLine();
#endregion
