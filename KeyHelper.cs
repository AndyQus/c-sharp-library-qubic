using System;
using System.Text;

public class KeyHelper
{
    private const string SEED_ALPHABET = "abcdefghijklmnopqrstuvwxyz";
    private const int PRIVATE_KEY_LENGTH = 32;
    private const int PUBLIC_KEY_LENGTH = 32;
    private const int CHECKSUM_LENGTH = 3;

    public byte[] CreatePublicKey(byte[] privateKey, dynamic schnorrq, dynamic K12)
    {
        byte[] publicKeyWithChecksum = new byte[PUBLIC_KEY_LENGTH + CHECKSUM_LENGTH];
        Array.Copy(schnorrq.GeneratePublicKey(privateKey), publicKeyWithChecksum, PUBLIC_KEY_LENGTH);
        K12(publicKeyWithChecksum.AsSpan(0, PUBLIC_KEY_LENGTH).ToArray(), publicKeyWithChecksum, CHECKSUM_LENGTH, PUBLIC_KEY_LENGTH);
        return publicKeyWithChecksum;
    }

    private byte[] SeedToBytes(string seed)
    {
        byte[] bytes = new byte[seed.Length];
        for (int i = 0; i < seed.Length; i++)
        {
            bytes[i] = (byte)SEED_ALPHABET.IndexOf(seed[i]);
        }
        return bytes;
    }

    public byte[] PrivateKey(string seed, int index, dynamic K12)
    {
        byte[] byteSeed = SeedToBytes(seed);
        byte[] preimage = new byte[byteSeed.Length];
        Array.Copy(byteSeed, preimage, byteSeed.Length);

        while (index-- > 0)
        {
            for (int i = 0; i < preimage.Length; i++)
            {
                if (++preimage[i] > SEED_ALPHABET.Length)
                {
                    preimage[i] = 1;
                }
                else
                {
                    break;
                }
            }
        }

        byte[] key = new byte[PRIVATE_KEY_LENGTH];
        K12(preimage, key, PRIVATE_KEY_LENGTH);
        return key;
    }

    public static byte[] GetIdentityBytes(string identity)
    {
        byte[] publicKeyBytes = new byte[32];
        var view = new DataView(publicKeyBytes);

        for (int i = 0; i < 4; i++)
        {
            view.SetBigUint64(i * 8, 0, true);
            for (int j = 14; j-- > 0;)
            {
                view.SetBigUint64(i * 8, view.GetBigUint64(i * 8, true) * 26 + (ulong)(identity[i * 14 + j] - 'A'), true);
            }
        }

        return publicKeyBytes;
    }
}

public class DataView
{
    private readonly byte[] buffer;

    public DataView(byte[] buffer)
    {
        this.buffer = buffer;
    }

    public void SetBigUint64(int byteOffset, ulong value, bool littleEndian)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian != littleEndian)
        {
            Array.Reverse(bytes);
        }
        Array.Copy(bytes, 0, buffer, byteOffset, 8);
    }

    public ulong GetBigUint64(int byteOffset, bool littleEndian)
    {
        byte[] bytes = new byte[8];
        Array.Copy(buffer, byteOffset, bytes, 0, 8);
        if (BitConverter.IsLittleEndian != littleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt64(bytes, 0);
    }
}
