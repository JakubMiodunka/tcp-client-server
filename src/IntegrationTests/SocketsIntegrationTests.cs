using NUnit.Framework.Internal;
using System.Net;
using TcpClientServer.Client;
using TcpClientServer.Common.Encryption;
using TcpClientServer.Common.Protocols;
using TcpClientServer.Server;

namespace TcpClientServer.IntegrationTests;

/// <remarks>
/// In current configuration maximal length of data, which can be transferred between client and server
/// is 65024 bytes. It's caused by header length of SSLPv2, which is set to 2 bytes.
/// </remarks>
[Category("IntegrationTest")]
[NonParallelizable]
[Author("Jakub Miodunka")]
public class SocketsIntegrationTests
{
    #region Constants
    private const string ServerIpAddress = "127.0.0.1";
    private const int ServerPort = 8888;

    private const int ConnectionStateChangeTimeout = 250;
    private const int DataTransferTimeout = 500;
    #endregion

    #region Properties
    private ServerSocket _server;
    private List<ClientSocket> _clients;
    #endregion

    #region Static methods
    private static IProtocol CreateProtocol()
    {
        const int HeaderLength = 2;

        return new SSLPv2(HeaderLength);
    }

    private static ICipher CreateCipher() =>
        new TransparentCipher();

    private static ServerSocket CreateServer()
    {
        const int ServerReceivingBufferSize = 1024;
        var serverIpAddress = IPAddress.Parse(ServerIpAddress);
        var serverEndPoint = new IPEndPoint(serverIpAddress, ServerPort);
        IProtocol protocol = CreateProtocol();
        ICipher cipher = CreateCipher();

        return new ServerSocket(serverEndPoint, ServerReceivingBufferSize, protocol, cipher);
    }

    private ClientSocket CreateClient()
    {
        const int ClientReceivingBufferSize = 1024;
        var serverIpAddress = IPAddress.Parse(ServerIpAddress);
        var serverEndPoint = new IPEndPoint(serverIpAddress, ServerPort);
        IProtocol protocol = CreateProtocol();
        ICipher cipher = CreateCipher();

        var client =  new ClientSocket(serverEndPoint, ClientReceivingBufferSize, protocol, cipher);
        _clients.Add(client);

        return client;
    }
    #endregion

    #region Test setup
    [SetUp]
    public void Setup()
    {
        _server = CreateServer();
        _clients = new List<ClientSocket>();
    }

    [TearDown]
    public void TearDown()
    {
        _clients.ForEach(clientSocket => clientSocket.Dispose());
        _server.Dispose();
    }
    #endregion

    #region Test cases
    [Test]
    public void ServerAcceptsClients([Values(1, 2, 40)] int numberOfClients)
    {
        _server.StartAcceptingConnections();

        for (int i = 0; i < numberOfClients; i++)
        {
            ClientSocket client = CreateClient();
            client.ConnectToServer();
        }

        int timeout = ConnectionStateChangeTimeout * numberOfClients;
        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(numberOfClients).After(timeout));
    }

    [Test]
    public void ServerTransfersDataToClient([Values(1, 10, 65_024)] int dataLength)
    {
        _server.StartAcceptingConnections();

        ClientSocket client = CreateClient();
        client.ConnectToServer();
        
        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(1).After(ConnectionStateChangeTimeout));

        Randomizer randomizer = TestContext.CurrentContext.Random;
        var sentData = new byte[dataLength];
        randomizer.NextBytes(sentData);

        int connectionIdentifier = _server.ActiveConnections.First();
        _server.SentData(connectionIdentifier, sentData);

        Assert.That(() => client.GetReceivedData()?.SequenceEqual(sentData), Is.True.After(DataTransferTimeout));
    }

    [Test]
    public void TransferTooLargeDataFromServerToClientImpossible([Values(65_025)] int dataLength)
    {
        _server.StartAcceptingConnections();

        ClientSocket client = CreateClient();
        client.ConnectToServer();

        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(1).After(ConnectionStateChangeTimeout));

        Randomizer randomizer = TestContext.CurrentContext.Random;
        var sentData = new byte[dataLength];
        randomizer.NextBytes(sentData);

        int connectionIdentifier = _server.ActiveConnections.First();
        TestDelegate actionUnderTest = () => _server.SentData(connectionIdentifier, sentData);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void ServerReactsWhenClientsAreClosingConnections([Values(1, 2, 40)] int numberOfClients)
    {
        _server.StartAcceptingConnections();

        for (int i = 0; i < numberOfClients; i++)
        {
            ClientSocket client = CreateClient();
            client.ConnectToServer();
        }

        int timeout = ConnectionStateChangeTimeout * numberOfClients;
        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(numberOfClients).After(timeout));

        _clients.ForEach(client => client.Dispose());

        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(0).After(timeout));
    }

    [Test]
    public void ClientTransfersDataToServer([Values(1, 10, 65_024)] int dataLength)
    {
        _server.StartAcceptingConnections();

        ClientSocket client = CreateClient();
        client.ConnectToServer();
        
        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(1).After(ConnectionStateChangeTimeout));

        Randomizer randomizer = TestContext.CurrentContext.Random;
        var sentData = new byte[dataLength];
        randomizer.NextBytes(sentData);
        
        client.SentData(sentData);

        Assert.That(() => _server.GetReceivedData()?.Content.SequenceEqual(sentData), Is.True.After(DataTransferTimeout));
    }

    [Test]
    public void TransferTooLargeDataFromClientToServerImpossible([Values(65_025)] int dataLength)
    {
        _server.StartAcceptingConnections();

        ClientSocket client = CreateClient();
        client.ConnectToServer();

        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(1).After(ConnectionStateChangeTimeout));

        Randomizer randomizer = TestContext.CurrentContext.Random;
        var sentData = new byte[dataLength];
        randomizer.NextBytes(sentData);

        TestDelegate actionUnderTest = () => client.SentData(sentData);

        Assert.Throws<ArgumentException>(actionUnderTest);
    }

    [Test]
    public void ClientReactsWhenServerClosesConnection()
    {
        _server.StartAcceptingConnections();

        ClientSocket client = CreateClient();
        client.ConnectToServer();

        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(1).After(ConnectionStateChangeTimeout));

        int connectionIdentifier = _server.ActiveConnections.First();
        _server.CloseConnection(connectionIdentifier);
        
        Assert.That(() => client.IsConnectionEstablished, Is.False.After(ConnectionStateChangeTimeout));
    }

    [Test]
    public void ClientReactsWhenServerIsBeingDisposed()
    {
        _server.StartAcceptingConnections();

        ClientSocket client = CreateClient();
        client.ConnectToServer();

        Assert.That(() => _server.ActiveConnections.Count(), Is.EqualTo(1).After(ConnectionStateChangeTimeout));

        _server.Dispose();

        Assert.That(() => client.IsConnectionEstablished, Is.False.After(ConnectionStateChangeTimeout));
    }
    #endregion
}
