using Common.Encryption;
using Common.Protocols;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Common;

/// <summary>
/// Implementation of TCP socket, capable to transfer encrypted data in full-duplex manner
/// (to send and receive encrypted data simultaneously).
/// </summary>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.connected?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receiveasync?view=net-9.0"/
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.sendasync?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.shutdown?view=net-9.0"/>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.close?view=net-9.0"/>
public abstract class TcpSocket : IDisposable
{
    #region Constants
    private const int ListeningForDataInterval = 200;   // Expressed in milliseconds.
    private const int SendingDataInterval = 200;        // Expressed in milliseconds.
    #endregion

    #region Properties
    private readonly int _receivingBufferSize;
    private readonly IProtocol _protocol;
    private readonly ICipher _cipher;
    private Task? _listeningForDataTask;
    private readonly CancellationTokenSource _cancellationTokenSourceForDataListening;
    private Task? _sendingDataTask;
    private readonly CancellationTokenSource _cancellationTokenSourceForDataSending;

    protected abstract Socket Socket { get; }
    protected abstract ConcurrentQueue<byte[]> SendingQueue { get; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Initializes functionalities of TCP socket.
    /// </summary>
    /// <param name="receivingBufferSize">
    /// Size of a buffer, used for buffering incoming data.
    /// </param>
    /// <param name="protocol">
    /// Session layer protocol, which shall be used during communication.
    /// </param>
    /// <param name="cipher">
    /// Cipher, which shall be used during communication to encrypt and decrypt data. 
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown, when value of at least one argument will be considered as invalid.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    protected TcpSocket(int receivingBufferSize, IProtocol protocol, ICipher cipher)
    {
        #region Arguments validation
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
        _cancellationTokenSourceForDataListening = new CancellationTokenSource();
        _cancellationTokenSourceForDataSending = new CancellationTokenSource();
    }
    #endregion

    #region Interactions
    /// <summary>
    /// Defines reaction on event, when connection will be closed by remote resource
    /// </summary>
    /// <remarks>
    /// Shall implement simple logic ex. set a property value.
    /// </remarks>
    protected abstract void ReactOnRemoteConnectionClose();

    /// <summary>
    /// Defines how data received from remote resource data shall be processed.
    /// </summary>
    /// <param name="data">
    /// Data received from remote resource.
    /// </param>
    protected abstract void ProcessReceivedData(IEnumerable<byte> data);

    /// <summary>
    /// Triggers continues process of listening for new patches of data on socket.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token, which shall be bound to launched task.
    /// </param>
    /// <returns>
    /// Task related to pending operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when socket is not connected to remote resource.
    /// </exception>
    private async Task StartListeningForData(CancellationToken cancellationToken)
    {
        #region Arguments validation
        if (!Socket.Connected)
        {
            const string ErrorMessage = "Socket not connected to remote resource:";
            throw new InvalidOperationException(ErrorMessage);
        }
        #endregion

        Task<int>? receivingTask = null;
        var receivedData = new List<byte>();
        var receivingBuffer = new byte[_receivingBufferSize];

        while (Socket.Connected && !cancellationToken.IsCancellationRequested)
        {
            receivingTask ??= Socket.ReceiveAsync(receivingBuffer);

            if (!receivingTask.IsCompleted)
            {
                await Task.Delay(ListeningForDataInterval);
                continue;
            }

            int sizeOfReceivedDataChunk = receivingTask.Result;
            receivingTask = null;

            // Receiving 0 bytes is an indicator, that remote resource closed its socket.
            if (sizeOfReceivedDataChunk == 0)
            {
                ReactOnRemoteConnectionClose();
                return;
            }

            byte[] receivedDataChunk = receivingBuffer.Take(sizeOfReceivedDataChunk).ToArray();
            receivedData.AddRange(receivedDataChunk);

            byte[] encryptedPayload;

            try
            {
                encryptedPayload = _protocol.ExtractPayload(receivedData);
            }
            catch (ArgumentException)
            {
                continue;
            }

            receivedData.Clear();

            byte[] decryptedPayload = _cipher.Decrypt(encryptedPayload);
            ProcessReceivedData(decryptedPayload);
        }
    }

    /// <summary>
    /// Triggers continues process of sending data present in dedicated queue through the socket.
    /// </summary>
    /// <returns>
    /// Task related to pending operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown, when socket is not connected to remote resource.
    /// </exception>
    private async Task StartSendingData(CancellationToken cancellationToken)
    {
        #region Arguments validation
        if (!Socket.Connected)
        {
            const string ErrorMessage = "Socket not connected to remote resource:";
            throw new InvalidOperationException(ErrorMessage);
        }
        #endregion

        Task<int>? sendingPacketTask = null;
        
        while (Socket.Connected && !cancellationToken.IsCancellationRequested)
        {
            if (sendingPacketTask is null)
            {
                if (SendingQueue.TryDequeue(out byte[]? messageContent))
                {
                    byte[] encryptedMessageContent = _cipher.Encrypt(messageContent);
                    byte[] packet = _protocol.PreparePacket(encryptedMessageContent);
                    sendingPacketTask = Socket.SendAsync(packet);
                    continue;
                }
            }
            else
            {
                if (sendingPacketTask.IsCompleted)
                {
                    sendingPacketTask = null;
                    continue;
                }
            }

            await Task.Delay(SendingDataInterval);
        }
    }

    /// <summary>
    /// Starts full-duplex data transfer between local and remote resource. 
    /// </summary>
    /// <remarks>
    /// Socket shall be connected to remote resource by derivative class before calling this method.
    /// </remarks>
    protected void StartDataTransfer()
    {
        _listeningForDataTask = StartListeningForData(_cancellationTokenSourceForDataListening.Token);
        _sendingDataTask = StartSendingData(_cancellationTokenSourceForDataSending.Token);
    }

    /// <summary>
    /// Suppresses currently pending sending and receiving operations on socket and dispose the socket itself.
    /// </summary>
    public virtual void Dispose()
    {
        if (_sendingDataTask is not null)
        {
            _cancellationTokenSourceForDataSending.Cancel();
            _sendingDataTask.Wait();

            _sendingDataTask = null;
        }

        if (_listeningForDataTask is not null)
        {
            _cancellationTokenSourceForDataListening.Cancel();
            _listeningForDataTask.Wait();

            _listeningForDataTask = null;
        }

        // Value of Socket.Connected property is not depend on state of connected remote resource.
        // Is set to 'true' during runtime of Socket.Connect method and to 'false', when socket is being disposed.
        // Is not updated when remote resource will close/dispose its socket.
        if (Socket.Connected)
        {
            // For connection-oriented protocols (such as TCP),
            // it is recommended to call Socket.Shutdown on connected socket before disposing it (calling socket.Close()).
            Socket.Shutdown(SocketShutdown.Both);
        }
        Socket.Close(); // Calls Socket.Dispose() internally.
    }
    #endregion
}
