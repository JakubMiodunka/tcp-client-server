using System.Net;

namespace Common;

/// <summary>
/// Model of message, which is transfered via socket connection.
/// </summary>
/// <param name="SenderEndPoint">
/// IP end point of message sender.
/// </param>
/// <param name="ReceiverEndPoint">
/// IP end point of message receiver.
/// </param>
/// <param name="Content">
/// Binary content of the message.
/// </param>
public sealed record SocketMessage(IPEndPoint SenderEndPoint, IPEndPoint ReceiverEndPoint, byte[] Content);
