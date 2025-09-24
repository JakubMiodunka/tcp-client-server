using Common.Encryption;
using Common.Padding;
using Common.Protocols;
using Server;
using System.Net;
using System.Text;

// Configuration:
var listeningEndPoint = new IPEndPoint(IPAddress.Loopback, 8888);
int receivingBufferSize = 1024;
var protocol = new SimpleSessionLayerProtocol(4);
var encryptionKey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
var bitPaddingProvider = new PkcsBitPaddingProvider(TeaCipher.DataBlockSize);
var cipher = new TeaCipher(encryptionKey, bitPaddingProvider);

// Main:
Console.WriteLine("Starting the server...");

using (var server = new ServerSocket(listeningEndPoint, receivingBufferSize, protocol, cipher))
{
    server.StartAcceptingConnections();
    Console.WriteLine("Listening for connections...");

    while (true)
    {
        SocketMessage? message = server.GetReceivedData();

        if (message.HasValue)
        {
            int connectionIdentifier = message.Value.ConnectionIdentifier;
            byte[] binaryContent = message.Value.Content.ToArray();

            string textContent = Encoding.UTF8.GetString(binaryContent);
            Console.WriteLine($"New message from connection {connectionIdentifier}: {textContent}");

            server.SentData(connectionIdentifier, binaryContent);
        }

        Thread.Sleep(200);
    }
}
