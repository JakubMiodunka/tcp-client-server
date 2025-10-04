using Common.Encryption;
using Common.Padding;
using Common.Protocols;
using Server;
using System.Net;
using System.Text;

#region Configuration
var listeningEndPoint = new IPEndPoint(IPAddress.Loopback, 8888);
int receivingBufferSize = 1024;
var protocol = new SSLPv1(4);
var encryptionKey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
var bitPaddingProvider = new PkcsBitPaddingProvider(TeaCipher.DataBlockSize);
var cipher = new TeaCipher(encryptionKey, bitPaddingProvider);
#endregion

#region Methods
async Task MonitorActiveConnections(ServerSocket serverSocket, CancellationToken cancellationToken)
{
    #region Arguments validation
    if (serverSocket is null)
    {
        string argumentName = nameof(serverSocket);
        const string ErrorMessage = "Provided server socket a null reference:";
        throw new ArgumentNullException(argumentName, ErrorMessage);
    }
    #endregion

    var lastActiveConnections = Array.Empty<int>();
    var currentlyActiveConnections = Array.Empty<int>();

    while (!cancellationToken.IsCancellationRequested)
    {
        lastActiveConnections = currentlyActiveConnections;
        currentlyActiveConnections = serverSocket.ActiveConnections;
        
        List<int> openedConnections = currentlyActiveConnections
            .Except(lastActiveConnections)
            .ToList();

        openedConnections.ForEach(connectionIdentifier => Console.WriteLine($"New connection opened: {connectionIdentifier}"));

        List<int> closedConnections = lastActiveConnections
            .Except(currentlyActiveConnections)
            .ToList();

        closedConnections.ForEach(connectionIdentifier => Console.WriteLine($"Connection closed: {connectionIdentifier}"));

        await Task.Delay(200);
    }
}

async Task EchoIncomingMessages(ServerSocket serverSocket, CancellationToken cancellationToken)
{
    #region Arguments validation
    if (serverSocket is null)
    {
        string argumentName = nameof(serverSocket);
        const string ErrorMessage = "Provided server socket a null reference:";
        throw new ArgumentNullException(argumentName, ErrorMessage);
    }
    #endregion

    while (!cancellationToken.IsCancellationRequested)
    {
        SocketMessage? message = serverSocket.GetReceivedData();

        if (message.HasValue)
        {
            int connectionIdentifier = message.Value.ConnectionIdentifier;
            byte[] binaryContent = message.Value.Content.ToArray();

            string textContent = Encoding.UTF8.GetString(binaryContent);
            Console.WriteLine($"New message from connection {connectionIdentifier}: {textContent}");

            serverSocket.SentData(connectionIdentifier, binaryContent);
            Console.WriteLine($"Message sent back to connection {connectionIdentifier}: {textContent}");
        }

        await Task.Delay(200);
    }
}
#endregion

#region Main
var tasks = new List<Task>();
var cancelationTokenSource = new CancellationTokenSource();

Console.WriteLine("Starting the server...");
using (var server = new ServerSocket(listeningEndPoint, receivingBufferSize, protocol, cipher))
{
    Task monitoringTask = MonitorActiveConnections(server, cancelationTokenSource.Token);
    tasks.Add(monitoringTask);

    Task echoingIncomingMessagesTask = EchoIncomingMessages(server, cancelationTokenSource.Token);
    tasks.Add(monitoringTask);

    server.StartAcceptingConnections();
    Console.WriteLine("Listening for connections...");

    while (true)
    {
        string? input = Console.ReadLine();

        if (input is not null)
        {
            if (input == "end")
            {
                break;
            }
        }
    }

    Console.WriteLine("Shutting down the server...");

    cancelationTokenSource.Cancel();
    Task.WaitAll(tasks.ToArray());
}

Console.WriteLine("Press ENTER to continue...");
Console.ReadLine();
#endregion
