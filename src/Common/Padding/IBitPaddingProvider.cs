namespace TcpClientServer.Common.Padding;

/// <summary>
/// Shall be implemented by all classes, which implements a bit padding.
/// </summary>
public interface IBitPaddingProvider
{
    int SizeOfDataBlock { get; }

    /// <summary>
    /// Adds bit padding to the given data set to make its length a multiple of the block size.
    /// </summary>
    /// <param name="data">
    /// Data set, to which bit padding shall be added.
    /// </param>
    /// <returns>
    /// Provided data set, with added bit padding.
    /// </returns>
    byte[] AddBitPadding(IEnumerable<byte> data);

    /// <summary>
    /// Removes padding from provided data set.
    /// </summary>
    /// <param name="data">
    /// Set of data, from which bit padding shall be removed.
    /// </param>
    /// <returns>
    /// Provided data set, with removed bit padding.
    /// </returns>
    byte[] RemoveBitPadding(IEnumerable<byte> data);
}
