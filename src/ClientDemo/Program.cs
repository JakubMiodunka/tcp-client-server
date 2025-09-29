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
Task? messageNotificationTask;
var cancelationTokenSource = new CancellationTokenSource();

Console.WriteLine("Starting the client...");
using (var client = new ClientSocket(serverEndPoint, receivingBufferSize, protocol, cipher))
{
    messageNotificationTask = NotifyAboutIncomingMessages(client, cancelationTokenSource.Token);

    Console.WriteLine("Connecting to server...");
    client.ConnectToServer();

    while (!client.IsConnectionEstablished)
    {
        Thread.Sleep(200);
    }

    Console.WriteLine("Connected to server.");

    while (true)
    {
        string? input = Console.ReadLine();

        if (input is not null)
        {
            byte[] inputAsBytes = Encoding.UTF8.GetBytes(input);

            if (input == "end") // To perform graceful shutdown.
            {
                cancelationTokenSource.Cancel();
                // TODO: Wait for messageNotificationTask to complete.
                break;
            }

            client.SentData(inputAsBytes);
        }
    }
}

Console.WriteLine("Press ENTER to continue...");
Console.ReadLine();
#endregion
