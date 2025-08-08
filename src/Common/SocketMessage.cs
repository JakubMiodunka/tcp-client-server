using System.Net;

namespace Common;

/// <summary>
/// Model of message, which is transfered via socket connection.
/// </summary>
/// <param name="SenderEndPoint">
/// IP end point of message sender.
/// If sender is unknown, this property will be set to null reference.
/// </param>
/// <param name="Content">
/// Binary content of the message.
/// </param>
public sealed record SocketMessage(IPEndPoint? SenderEndPoint, byte[] Content);
