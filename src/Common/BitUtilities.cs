namespace TcpClientServer.Common;

/// <summary>
/// Set of utilities related to bitwise operations.
/// </summary>
public static class BitUtilities
{
    /// <summary>
    /// Transforms provided collection of bytes to bitwise equivalent
    /// collection of unsigned integers.
    /// </summary>
    /// <param name="data">
    /// Collection of bytes, which shall be transformed.
    /// </param>
    /// <returns>
    /// Bitwise equivalent of provided collection of bytes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown, when at least one argument will be considered as invalid.
    /// </exception>
    public static uint[] AsUintArray(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data set is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }

        if ((data.Count() % 4) != 0)
        {
            string argumentName = nameof(data);
            string errorMessage = $"Invalid length of provided data set: {data.Count()}";
            throw new ArgumentException(errorMessage, argumentName);
        }
        #endregion

        return data
            .Chunk(4)
            .Select(dataChunk => BitConverter.ToUInt32(dataChunk, 0))
            .ToArray();
    }
}
