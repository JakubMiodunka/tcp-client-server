using Common;
using Common.Encryption;
using Common.Protocols;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Server.Sockets;

/// <summary>
/// Wrapper for main server socket, which accepts incoming connections.
/// It accepts connections, manages them and provide mechanisms to receive and send data from/to all connected clients.
/// </summary>
/// <remarks>
/// To get it to operational state properly, first instantiate the class member and StartAcceptingConnections() method to start accepting new clients.
/// After establishing connection, methods SentData and GetReceivedData can be used to send and receive data respectively.
/// Monitor value of ClientEndPoints to be aware about pool of currently connected clients.
/// Do not forget to dispose created instance, when it will be no longer needed.
/// </remarks>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.bind?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.listen?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.acceptasync?view=net-9.0"/>
public sealed class ServerSocket : IDisposable
{
    #region Constants
    private const int ListeningForConnectionInterval = 200;   // Expressed in milliseconds.
    #endregion

    #region Properties
    private readonly int _receivingBufferSize;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;
    private readonly Socket _listeningSocket;
    private readonly List<ConnectionSocket> _connectionSockets;
    private readonly ConcurrentQueue<SocketMessage> _receivingQueue;
    private Task? _acceptingConnectionsTask;
    private readonly CancellationTokenSource _cancellationTokenSourceForAcceptingConnections;

    public IPEndPoint[] RemoteEndPoints
    {
        get
        {
            IEnumerable<IPEndPoint> endPoints;

            lock (_connectionSockets)
            {
                endPoints =  from socket in _connectionSockets.Select(socket => socket.RemoteEndPoint)
                             where socket is not null
                             select socket;
            }

            return endPoints.ToArray();
        }
    }
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes server socket wrapper.
    /// </summary>
    /// <param name="listeningEndPoint">
    /// End point, to which listening socket shall bind to.
    /// </param>
    /// <param name="receivingBufferSize">
    /// Size of a buffer, used by every connection handle for buffering data incoming from particular client.
    /// Expressed in bytes.
    /// </param>
    /// <param name="protocol">
    /// Session layer protocol, which shall be used during communication with each client.
    /// </param>
    /// <param name="cipher">
    /// Cipher, which shall be used during communication with each client to encrypt and decrypt data. 
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    public ServerSocket(IPEndPoint listeningEndPoint, int receivingBufferSize, IProtocol protocol, ICipher cipher) : base()
    {
        #region Arguments validation
        if (listeningEndPoint is null)
        {
            string argumentName = nameof(listeningEndPoint);
            const string ErrorMessage = "Provided IP end point is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (receivingBufferSize < 1)
        {
            string argumentName = nameof(receivingBufferSize);
            string errorMessage = $"Specified size of receiving buffer too small: {receivingBufferSize}";
            throw new ArgumentOutOfRangeException(argumentName, receivingBufferSize, errorMessage);
        }

        if (protocol is null)
        {
            string argumentName = nameof(protocol);
            const string ErrorMessage = "Provided protocol is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (cipher is null)
        {
            string argumentName = nameof(cipher);
            const string ErrorMessage = "Provided cipher is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _receivingBufferSize = receivingBufferSize;
        _protocol = protocol;
        _cipher = cipher;
        _listeningSocket = new Socket(listeningEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _connectionSockets = new List<ConnectionSocket>();
        _receivingQueue = new ConcurrentQueue<SocketMessage>();
        _acceptingConnectionsTask = null;
        _cancellationTokenSourceForAcceptingConnections = new CancellationTokenSource();

        _listeningSocket.Bind(listeningEndPoint);
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Creates new internally managed wrapper for newly accepted connection.
    /// </summary>
    /// <remarks>
    /// Additionally, whenever this method is being invoked, inactive connection socket wrappers are disposed.
    /// It is simple yet effective mechanism of lazy-management of the connection pool.
    /// </remarks>
    /// <param name="connectionSocket">
    /// Socket referring to newly accepted connection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    private void PrepareWrapperForConnectionSocket(Socket connectionSocket)
    {
        #region Arguments validation
        if (connectionSocket is null)
        {
            string argumentName = nameof(connectionSocket);
            const string ErrorMessage = "Provided socket a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        var socketWrapper = new ConnectionSocket(connectionSocket, _receivingBufferSize, _protocol, _cipher, _receivingQueue);
        
        lock(_connectionSockets)
        {
            List<ConnectionSocket> inactiveSockets = _connectionSockets.Where(socket => !socket.IsConnectionEstablished).ToList();
            inactiveSockets.ForEach(socket => socket.Dispose());
            inactiveSockets.ForEach(socket => _connectionSockets.Remove(socket));
            
            _connectionSockets.Add(socketWrapper);
        }

        socketWrapper.StartDataTransfer();
    }

    /// <summary>
    /// Triggers continues process of listening and accepting new connections on listening socket.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token, which shall be bound to launched task.
    /// </param>
    /// <returns>
    /// Task related to pending operation.
    /// </returns>
    private async Task StartAcceptingConnections(CancellationToken cancellationToken)
    {
        _listeningSocket.Listen();

        Task<Socket>? acceptNewConnectionTask = null;

        while (_listeningSocket.IsBound && !cancellationToken.IsCancellationRequested)
        {
            acceptNewConnectionTask ??= _listeningSocket.AcceptAsync();

            if (!acceptNewConnectionTask.IsCompleted)
            {
                await Task.Delay(ListeningForConnectionInterval);
                continue;
            }

            Socket connectionSocket = acceptNewConnectionTask.Result;
            acceptNewConnectionTask = null;

            PrepareWrapperForConnectionSocket(connectionSocket);
        }
    }

    /// <summary>
    /// Triggers continues process of listening and accepting new connections on listening socket.
    /// </summary>
    public void StartAcceptingConnections()
    {
        _acceptingConnectionsTask = StartAcceptingConnections(_cancellationTokenSourceForAcceptingConnections.Token);
    }

    /// <summary>
    /// Sends provided data to specified connection.
    /// </summary>
    /// <param name="clientEndPoint">
    /// End point of a client, to which provided data shall be transfered.
    /// </param>
    /// <param name="data">
    /// Data, which shall be sent to specified client.
    /// </param>
    /// <returns>
    /// True, if data was sent to client related with specified end point, false otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public bool SentData(IPEndPoint clientEndPoint, IEnumerable<byte> data)
    {
        #region Arguments validation
        if (clientEndPoint is null)
        {
            string argumentName = nameof(clientEndPoint);
            const string ErrorMessage = "Provided IP end point is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        ConnectionSocket? connectionSocket;

        lock (_connectionSockets)
        {
            // Overload of '==' operator not defined in IPEndPoint, but Equals method implements suitable end points comparison.
            connectionSocket = _connectionSockets
                .FirstOrDefault(socket => clientEndPoint.Equals(socket.RemoteEndPoint));
        }

        connectionSocket?.SentData(data);

        return connectionSocket is not null;
    }

    /// <summary>
    /// Returns first element of receiving queue.
    /// </summary>
    /// <returns>
    /// First element of receiving queue, if it is not empty, null reference otherwise.
    /// </returns>
    public SocketMessage? GetReceivedData()
    {
        if (_receivingQueue.TryDequeue(out var receivedMessage))
        {
            return receivedMessage;
        }

        return null;
    }


    /// <summary>
    /// Closes connection with specified identifier.
    /// </summary>
    /// <param name="clientEndPoint">
    /// End point of a client, whose connection shall be closed.
    /// </param>
    /// <returns>
    /// True, if connection related to specified end point was closed, false otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public bool CloseConnection(IPEndPoint clientEndPoint)
    {
        #region Arguments validation
        if (clientEndPoint is null)
        {
            string argumentName = nameof(clientEndPoint);
            const string ErrorMessage = "Provided IP end point is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        ConnectionSocket? connectionSocket;

        lock (_connectionSockets)
        {
            // Overload of '==' operator not defined in IPEndPoint, but Equals method implements suitable end points comparison.
            connectionSocket = _connectionSockets
                .FirstOrDefault(socket => clientEndPoint.Equals(socket.RemoteEndPoint));
        }

        if (connectionSocket is null)
        {
            return false;
        }

        connectionSocket.Dispose();

        lock (_connectionSockets)
        {
            _connectionSockets.Remove(connectionSocket);
        }

        return true;
    }

    /// <summary>
    /// Suppresses accepting new connections, disposes all connection sockets along with listening socket.
    /// </summary>
    /// <remarks>
    /// Calling Socket.Shutdown method is not necessary here (and will cause throwing an exception)
    /// as listening socket on server site is not connected (Socket.Connected property value is set
    /// to 'false') - it only listens for new connections and accepts them.
    /// For each connection new separate (and connected) socket is created to handle it individually.
    /// </remarks>
    public void Dispose()
    {
        if (_acceptingConnectionsTask is not null)
        {
            _cancellationTokenSourceForAcceptingConnections.Cancel();
            _acceptingConnectionsTask.Wait();

            _acceptingConnectionsTask = null;
        }

        _listeningSocket.Close();   // Calls Socket.Dispose() internally.

        lock (_connectionSockets)
        {
            _connectionSockets.ForEach(socket => socket.Dispose());
            _connectionSockets.Clear();
        }
    }
    #endregion
}
