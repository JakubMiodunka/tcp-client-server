namespace TcpClientServer.Common.Encryption;

/// <summary>
/// Shall be implemented by every cipher.
/// </summary>
public interface ICipher
{
    /// <summary>
    /// Encrypts provided data set.
    /// </summary>
    /// <param name="data">
    /// Data set, which shall be encrypted.
    /// </param>
    /// <returns>
    /// Set of encrypted data, corresponding to provided data set.
    /// </returns>
    byte[] Encrypt(IEnumerable<byte> data);

    /// <summary>
    /// Decrypts provided data set.
    /// </summary>
    /// <param name="data">
    /// Data, which shall be decrypted.
    /// </param>
    /// <returns>
    /// Set of decrypted data, corresponding to provided data set.
    /// </returns>
    byte[] Decrypt(IEnumerable<byte> data);
}
