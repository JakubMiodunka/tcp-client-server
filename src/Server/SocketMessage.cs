using System.Collections.ObjectModel;

namespace TcpClientServer.Server;

/// <summary>
/// Model of message, which is transfered via socket connection.
/// </summary>
/// <remarks>
/// Previously this model was implemented as record type, but it was more efficient to implement it as a structure.
/// Structures as a value types are allocated on stack instead of heap as records does, so access to them is quicker.
/// In our case the model does not contain large amounts of data, so necessity of coping it every time,
/// when instance is passed around does not cause performance issues.
/// Small amount of data to copy and shorter access to memory makes this model more efficient as a structure.
/// </remarks>
public readonly struct SocketMessage
{
    #region Properties
    public readonly int ConnectionIdentifier;
    public readonly ReadOnlyCollection<byte> Content;
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates a new message instance.
    /// </summary>
    /// <param name="connectionIdentifier">
    /// Unique identifier of message sender connection.
    /// </param>
    /// <param name="content">
    /// Binary content of the message.
    /// </param>
    public SocketMessage(int connectionIdentifier, IEnumerable<byte> content)
    {
        ConnectionIdentifier = connectionIdentifier;
        Content = content.ToArray().AsReadOnly();
    }
    #endregion
}
