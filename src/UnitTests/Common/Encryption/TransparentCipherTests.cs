using NUnit.Framework.Internal;
using TcpClientServer.Common.Encryption;

namespace TcpClientServer.UnitTests.Common.Ciphers;

[Category("UnitTest")]
[TestOf(typeof(TransparentCipher))]
[Author("Jakub Miodunka")]
public sealed class TransparentCipherTests
{
    #region Test cases
    [Test]
    public void InstantiationPossible()
    {
        TestDelegate actionUnderTest = () => new TransparentCipher();

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void EncryptionOfNullReferenceNotPossible()
    {
        var instanceUnderTest = new TransparentCipher();
        TestDelegate actionUnderTest = () => instanceUnderTest.Encrypt(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void DecryptionOfNullReferenceNotPossible()
    {
        var instanceUnderTest = new TransparentCipher();
        TestDelegate actionUnderTest = () => instanceUnderTest.Decrypt(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void EncryptionIsTransparent(
        [Values(0, 1, 56_897)] int numberOfBytesToProcess)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var inputDataSet = new byte[numberOfBytesToProcess];
        randomizer.NextBytes(inputDataSet);

        var instanceUnderTest = new TransparentCipher();

        byte[] encryptedDataSet = instanceUnderTest.Encrypt(inputDataSet);
        byte[] decryptedDataSet = instanceUnderTest.Decrypt(encryptedDataSet);

        Assert.That(decryptedDataSet.SequenceEqual(inputDataSet));
    }
    #endregion
}
