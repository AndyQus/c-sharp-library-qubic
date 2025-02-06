using System;
using System.Text;
using System.Threading.Tasks;

public class QubicPackageBuilder
{
    private byte[] packet;
    private int offset = 0;

    public QubicPackageBuilder(int size)
    {
        // todo: create a dynamic builder
        packet = new byte[size];
    }

    public QubicPackageBuilder()
    {
    }

    public byte[] GetData()
    {
        return packet;
    }

    public async Task<byte[]> Sign(string seed)
    {
        var crypto = await Crypto.GetInstance();
        KeyHelper keyHelper = new KeyHelper();

        byte[] privateKey = keyHelper.PrivateKey(seed, 0, crypto.K12);
        byte[] publicKey = keyHelper.CreatePublicKey(privateKey, crypto.Schnorrq, crypto.K12);

        byte[] digest = new byte[QubicDefinitions.DIGEST_LENGTH];
        byte[] toSign = new byte[offset];
        Array.Copy(packet, toSign, offset);

        crypto.K12(toSign, digest, QubicDefinitions.DIGEST_LENGTH);
        byte[] signature = crypto.Schnorrq.Sign(privateKey, publicKey, digest);

        Array.Copy(signature, 0, packet, offset, QubicDefinitions.SIGNATURE_LENGTH);
        offset += QubicDefinitions.SIGNATURE_LENGTH;

        byte[] signedData = new byte[offset];
        Array.Copy(packet, signedData, offset);
        return signedData;
    }

    public async Task<(byte[] signedData, byte[] digest, byte[] signature)> SignAndDigest(string seed)
    {
        var crypto = await Crypto.GetInstance();
        KeyHelper keyHelper = new KeyHelper();

        byte[] privateKey = keyHelper.PrivateKey(seed, 0, crypto.K12);
        byte[] publicKey = keyHelper.CreatePublicKey(privateKey, crypto.Schnorrq, crypto.K12);

        byte[] digest = new byte[QubicDefinitions.DIGEST_LENGTH];
        byte[] toSign = new byte[offset];
        Array.Copy(packet, toSign, offset);

        crypto.K12(toSign, digest, QubicDefinitions.DIGEST_LENGTH);
        byte[] signature = crypto.Schnorrq.Sign(privateKey, publicKey, digest);

        Array.Copy(signature, 0, packet, offset, QubicDefinitions.SIGNATURE_LENGTH);
        offset += QubicDefinitions.SIGNATURE_LENGTH;

        byte[] signedData = new byte[offset];
        Array.Copy(packet, signedData, offset);
        crypto.K12(signedData, digest, QubicDefinitions.DIGEST_LENGTH);

        return (signedData, digest, signature);
    }

    public QubicPackageBuilder Add(IQubicBuildPackage q)
    {
        byte[] data = q.GetPackageData();
        Array.Copy(data, 0, packet, offset, data.Length);
        offset += data.Length;
        return this;
    }

    public QubicPackageBuilder AddUint8Array(byte[] q)
    {
        return AddRaw(q);
    }

    public QubicPackageBuilder AddRaw(byte[] q)
    {
        Array.Copy(q, 0, packet, offset, q.Length);
        offset += q.Length;
        return this;
    }

    public QubicPackageBuilder AddShort(short q)
    {
        Array.Copy(BitConverter.GetBytes(q), 0, packet, offset, 2);
        offset += 2;
        return this;
    }

    public QubicPackageBuilder AddInt(int q)
    {
        Array.Copy(BitConverter.GetBytes(q), 0, packet, offset, 4);
        offset += 4;
        return this;
    }
}
