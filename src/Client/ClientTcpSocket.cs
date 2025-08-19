using Common;
using Common.Encryption;
using Common.Protocols;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;

namespace Client;

/// <summary>
/// Socket wrapper, which serves as a client in TCP client-server architecture.
/// Capable to transfer encrypted data in full-duplex manner.
/// </summary>
/// <remarks>
/// To get it to operational state properly, first instantiate the class member, then call ConnectToServer() method to start data transfer.
/// After establishing connection methods SentData and GetReceivedData can be used respectively to send and receive data.
/// Monitor value of IsConnectionEstablished to be aware about current state of established connection.
/// Do not forget to dispose created instance, when it will be no longer needed.
/// </remarks>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.connect?view=net-9.0"/>
public sealed class ClientSocket : TcpSocket
{
    #region Properties
    private readonly ConcurrentQueue<ReadOnlyCollection<byte>> _receivingQueue;

    protected override Socket Socket { get; }

    public readonly IPEndPoint TargetRemoteEndPoint;
    public bool IsConnectionEstablished { get; private set; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes client TCP socket.
    /// </summary>
    /// <param name="targetRemoteEndPoint">
    /// End point of a server, to which client shall connect.
    /// </param>
    /// <param name="receivingBufferSize">
    /// Size of a buffer, used for buffering data incoming from the server site.
    /// Expressed in bytes.
    /// </param>
    /// <param name="protocol">
    /// Session layer protocol, which shall be used during communication.
    /// </param>
    /// <param name="cipher">
    /// Cipher, which shall be used during communication to encrypt and decrypt data. 
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public ClientSocket(IPEndPoint targetRemoteEndPoint, int receivingBufferSize, IProtocol protocol, ICipher cipher)
        : base(receivingBufferSize, protocol, cipher)
    {
        #region Arguments validation
        if (targetRemoteEndPoint is null)
        {
            string argumentName = nameof(targetRemoteEndPoint);
            const string ErrorMessage = "Provided IP end point is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        TargetRemoteEndPoint = targetRemoteEndPoint;
        Socket = new Socket(targetRemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _receivingQueue = new ConcurrentQueue<ReadOnlyCollection<byte>>();
        IsConnectionEstablished = false;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Defines reaction on event, when connection will be closed on server site.
    /// </summary>
    protected override void ReactOnRemoteConnectionClose()
    {
        IsConnectionEstablished = false;
    }

    /// <summary>
    /// Adds data received from server to receiving queue.
    /// </summary>
    /// <param name="data">
    /// Data received from server site.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    protected override void ProcessReceivedData(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        _receivingQueue.Enqueue(data.ToArray().AsReadOnly());
    }

    /// <summary>
    /// Connects client socket to server and starts data transfer.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when socket is already connected to server.
    /// </exception>
    public void ConnectToServer()
    {
        #region Arguments validation
        if (Socket.Connected)
        {
            const string ErrorMessage = "Socket already connected to server:";
            throw new InvalidOperationException(ErrorMessage);
        }
        #endregion

        Socket.Connect(TargetRemoteEndPoint);    // Throws SocketException when connection will fail.
        StartDataTransfer();
        IsConnectionEstablished = true;
    }

    /// <summary>
    /// Returns first element of receiving queue.
    /// </summary>
    /// <returns>
    /// First element of receiving queue, if it is not empty, null reference otherwise.
    /// </returns>
    public byte[]? GetReceivedData()
    {
        if (_receivingQueue.TryDequeue(out var data))
        {
            return [.. data];
        }

        return null;
    }

    /// <summary>
    /// Suppresses currently pending sending and receiving operations on socket and dispose the socket itself.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();
        IsConnectionEstablished = false;
    }
    #endregion
}
