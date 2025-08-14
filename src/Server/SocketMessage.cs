namespace Server;

/// <summary>
/// Model of message, which is transfered via socket connection.
/// </summary>
/// <param name="ConnectionIdentifier">
/// Unique identifier of message sender connection.
/// </param>
/// <param name="Content">
/// Binary content of the message.
/// </param>
public sealed record SocketMessage(int ConnectionIdentifier, byte[] Content);
