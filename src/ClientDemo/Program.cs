// Configuration:
using Client;
using Common.Encryption;
using Common.Padding;
using Common.Protocols;
using System.Net;
using System.Text;

var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8888);
int receivingBufferSize = 1024;
var protocol = new SimpleSessionLayerProtocol(4);
var encryptionKey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
var bitPaddingProvider = new PkcsBitPaddingProvider(TeaCipher.DataBlockSize);
var cipher = new TeaCipher(encryptionKey, bitPaddingProvider);

// Main:
using (var client = new ClientSocket(serverEndPoint, receivingBufferSize, protocol, cipher))
{
    client.ConnectToServer();

    Console.WriteLine("Connecting to server...");

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

            if (input == "shutdown") // To perform graceful shutdown.
            {
                break;
            }

            client.SentData(inputAsBytes);
        }
    }
}

Console.WriteLine("Press ENTER to continue...");
Console.ReadLine();