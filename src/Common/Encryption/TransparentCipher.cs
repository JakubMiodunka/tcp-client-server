namespace TcpClientServer.Common.Encryption;

/// <summary>
/// Transparent cipher, which does not perform any encryption or decryption.
/// Created to utilize unencrypted comunication, mainly for testing purposes.
/// </summary>
public sealed class TransparentCipher : ICipher
{
    #region Interactions
    /// <summary>
    /// Passes provided data set without any modifications
    /// </summary>
    /// <param name="data">
    /// Input data set.
    /// </param>
    /// <returns>
    /// Unchanged input data set.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one reference-type argument is a null reference.
    /// </exception>
    private byte[] Pass(IEnumerable<byte> data)
    {
        #region Arguments validation
        if (data is null)
        {
            string argumentName = nameof(data);
            const string ErrorMessage = "Provided data set is a null reference:";
            throw new ArgumentNullException(argumentName, ErrorMessage);
        }
        #endregion

        return data.ToArray();
    }

    /// <summary>
    /// Mocks encryption of provided data set.
    /// </summary>
    /// <param name="data">
    /// Input data set.
    /// </param>
    /// <returns>
    /// Unchanged input data set.
    /// </returns>
    public byte[] Encrypt(IEnumerable<byte> data) =>
        Pass(data);

    /// <summary>
    /// Mocks decryption of provided data set.
    /// </summary>
    /// <param name="data">
    /// Input data set.
    /// </param>
    /// <returns>
    /// Unchanged input data set.
    /// </returns>
    public byte[] Decrypt(IEnumerable<byte> data) =>
        Pass(data);
    #endregion
}
