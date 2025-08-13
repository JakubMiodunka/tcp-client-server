using Common;
using Common.Encryption;
using Common.Protocols;
using System.Collections.Concurrent;
using System.Net.Sockets;


namespace Server;

/// <summary>
/// Socket wrapper, which handles individual connection accepted by main server socket.
/// Capable to transfer encrypted data in full-duplex manner.
/// </summary>
/// <remarks>
/// To get it to operational state properly, first instantiate the class member
/// and call StartDataTransfer() method to start data transfer.
/// Do not forget to dispose created instance, when it will be no longer needed.
/// </remarks>
internal sealed class ConnectionSocket : TcpSocket
{
    #region Properties
    protected override Socket Socket { get; }
    protected override ConcurrentQueue<byte[]> SendingQueue { get; }
    protected override ConcurrentQueue<SocketMessage> ReceivingQueue { get; }

    public bool IsConnectionEstablished { get; private set; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes connection socket wrapper.
    /// </summary>
    /// <param name="connectionSocket">
    /// Socket used to handle particular client connection.
    /// </param>
    /// <param name="receivingBufferSize">
    /// Size of a buffer, used for buffering data incoming from the client site.
    /// Expressed in bytes.
    /// </param>
    /// <param name="protocol">
    /// Session layer protocol, which shall be used during communication.
    /// </param>
    /// <param name="cipher">
    /// Cipher, which shall be used during communication to encrypt and decrypt data. 
    /// </param>
    /// <param name="receivingQueue">
    /// Reference to queue, to which all received messages shall be added.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    public ConnectionSocket(Socket connectionSocket, int receivingBufferSize, IProtocol protocol, ICipher cipher, ConcurrentQueue<SocketMessage> receivingQueue)
        : base(receivingBufferSize, protocol, cipher)
    {
        #region Arguments validation
        if (connectionSocket is null)
        {
            string argumentName = nameof(connectionSocket);
            const string ErrorMessage = "Provided socket is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if (!connectionSocket.Connected)
        {
            string argumentName = nameof(connectionSocket);
            const string ErrorMessage = "Provided socket not connected:";
            throw new ArgumentException(ErrorMessage, argumentName);
        }

        if (receivingQueue is null)
        {
            string argumentName = nameof(receivingQueue);
            const string ErrorMessage = "Provided queue is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        Socket = connectionSocket;
        IsConnectionEstablished = connectionSocket.Connected;
        SendingQueue = new ConcurrentQueue<byte[]>();
        ReceivingQueue = receivingQueue;
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Defines reaction on event, when connection will be closed on client site.
    /// </summary>
    protected override void ReactOnRemoteConnectionClose()
    {
        IsConnectionEstablished = false;
    }

    /// <summary>
    /// Adds provided data to sending queue.
    /// </summary>
    /// <param name="data">
    /// Data, which shall be sent to client.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    public void SentData(byte[] data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        SendingQueue.Enqueue(data);
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
