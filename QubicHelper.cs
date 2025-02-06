using System;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

public class QubicHelper
{
    private const string SEED_ALPHABET = "abcdefghijklmnopqrstuvwxyz";
    private const string SHIFTED_HEX_CHARS = "abcdefghijklmnop";
    private const int PRIVATE_KEY_LENGTH = 32;
    private const int PUBLIC_KEY_LENGTH = 32;
    private const int SEED_IN_LOWERCASE_LATIN_LENGTH = 55;
    private const int CHECKSUM_LENGTH = 3;
    private const int REQUEST_RESPONSE_HEADER_SIZE = 8;
    private const int TRANSACTION_SIZE = 144;
    private const int IPO_TRANSACTION_SIZE = 144 + 8 /*price*/ + 2 /* quantity */ + 6 /* padding */;
    private const int SET_PROPOSAL_AND_BALLOT_REQUEST_SIZE = 592;
    private const int SIGNATURE_LENGTH = 64;
    private const int DIGEST_LENGTH = 32;
    private const int SPECIAL_COMMAND_SHUT_DOWN = 0;
    private const int SPECIAL_COMMAND_GET_PROPOSAL_AND_BALLOT_REQUEST = 1;
    private const int SPECIAL_COMMAND_GET_PROPOSAL_AND_BALLOT_RESPONSE = 2;
    private const int SPECIAL_COMMAND_SET_PROPOSAL_AND_BALLOT_REQUEST = 3;
    private const int SPECIAL_COMMAND_SET_PROPOSAL_AND_BALLOT_RESPONSE = 4;
    private const int PROCESS_SPECIAL_COMMAND = 255;

    public async Task<(byte[] publicKey, byte[] privateKey, string publicId)> CreateIdPackage(string seed)
    {
        var crypto = await Crypto.GetInstance();
        var privateKey = PrivateKey(seed, 0, crypto.K12);
        var publicKey = crypto.Schnorrq.GeneratePublicKey(privateKey);
        var publicId = await GetIdentity(publicKey);
        return (publicKey, privateKey, publicId);
    }

    private async Task<byte[]> GetCheckSum(byte[] publicKey)
    {
        var crypto = await Crypto.GetInstance();
        var digest = new byte[QubicDefinitions.DIGEST_LENGTH];
        crypto.K12(publicKey, digest, QubicDefinitions.DIGEST_LENGTH);
        return digest.AsSpan(0, CHECKSUM_LENGTH).ToArray();
    }

    public async Task<string> GetIdentity(byte[] publicKey, bool lowerCase = false)
    {
        var newId = new StringBuilder();
        for (int i = 0; i < 4; i++)
        {
            var longNumber = new BigInteger(0);
            for (int j = 0; j < 8; j++)
            {
                longNumber += new BigInteger(publicKey[i * 8 + j]) * BigInteger.Pow(256, j);
            }
            for (int j = 0; j < 14; j++)
            {
                newId.Append((char)(longNumber % 26 + (lowerCase ? 'a' : 'A')));
                longNumber /= 26;
            }
        }

        var checksum = await GetCheckSum(publicKey);
        var identityBytesChecksum = (checksum[2] << 16) | (checksum[1] << 8) | checksum[0];
        identityBytesChecksum &= 0x3FFFF;

        for (int i = 0; i < 4; i++)
        {
            newId.Append((char)(identityBytesChecksum % 26 + (lowerCase ? 'a' : 'A')));
            identityBytesChecksum /= 26;
        }

        return newId.ToString();
    }

    public async Task<string> GetHumanReadableBytes(byte[] publicKey)
    {
        return await GetIdentity(publicKey, true);
    }

    private byte[] SeedToBytes(string seed)
    {
        var bytes = new byte[seed.Length];
        for (int i = 0; i < seed.Length; i++)
        {
            bytes[i] = (byte)SEED_ALPHABET.IndexOf(seed[i]);
        }
        return bytes;
    }

    public byte[] PrivateKey(string seed, int index, dynamic K12)
    {
        var byteSeed = SeedToBytes(seed);
        var preimage = new byte[byteSeed.Length];
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

        var key = new byte[PRIVATE_KEY_LENGTH];
        K12(preimage, key, PRIVATE_KEY_LENGTH);
        return key;
    }

    public static byte[] GetIdentityBytes(string identity)
    {
        var publicKeyBytes = new byte[32];
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

    public async Task<bool> VerifyIdentity(string identity)
    {
        if (string.IsNullOrEmpty(identity) || identity.Length != 60 || !System.Text.RegularExpressions.Regex.IsMatch(identity, "^[A-Z]+$"))
            return false;

        var publicKey = GetIdentityBytes(identity);
        var idFromBytes = await GetIdentity(publicKey);

        return identity == idFromBytes;
    }

    private byte[] CreatePublicKey(byte[] privateKey, dynamic schnorrq, dynamic K12)
    {
        var publicKeyWithChecksum = new byte[PUBLIC_KEY_LENGTH + CHECKSUM_LENGTH];
        Array.Copy(schnorrq.GeneratePublicKey(privateKey), publicKeyWithChecksum, PUBLIC_KEY_LENGTH);
        K12(publicKeyWithChecksum.AsSpan(0, PUBLIC_KEY_LENGTH).ToArray(), publicKeyWithChecksum, CHECKSUM_LENGTH, PUBLIC_KEY_LENGTH);
        return publicKeyWithChecksum;
    }

    public async Task<byte[]> CreateIpo(string sourceSeed, int contractIndex, long price, short quantity, int tick)
    {
        var crypto = await Crypto.GetInstance();
        var sourcePrivateKey = PrivateKey(sourceSeed, 0, crypto.K12);
        var sourcePublicKey = CreatePublicKey(sourcePrivateKey, crypto.Schnorrq, crypto.K12);

        var tx = new byte[IPO_TRANSACTION_SIZE];
        var txView = new DataView(tx);

        int offset = 0;
        Array.Copy(sourcePublicKey, 0, tx, offset, PUBLIC_KEY_LENGTH);
        offset += PUBLIC_KEY_LENGTH;

        tx[offset++] = (byte)contractIndex;
        offset += PUBLIC_KEY_LENGTH - 1;

        txView.SetBigInt64(offset, 0, true);
        offset += 8;

        txView.SetInt32(offset, tick, true);
        offset += 4;

        txView.SetInt16(offset, 1, true);
        offset += 2;

        txView.SetInt16(offset, 16, true);
        offset += 2;

        txView.SetBigInt64(offset, price, true);
        offset += 8;

        txView.SetInt16(offset, quantity, true);
        offset += 2;

        offset += 6;

        var digest = new byte[DIGEST_LENGTH];
        var toSign = new byte[offset];
        Array.Copy(tx, toSign, offset);

        crypto.K12(toSign, digest, DIGEST_LENGTH);
        var signedTx = crypto.Schnorrq.Sign(sourcePrivateKey, sourcePublicKey, digest);

        Array.Copy(signedTx, 0, tx, offset, SIGNATURE_LENGTH);
        offset += SIGNATURE_LENGTH;

        return tx;
    }

    public async Task<byte[]> CreateTransaction(string sourceSeed, string destPublicId, long amount, int tick)
    {
        var crypto = await Crypto.GetInstance();
        var sourcePrivateKey = PrivateKey(sourceSeed, 0, crypto.K12);
        var sourcePublicKey = CreatePublicKey(sourcePrivateKey, crypto.Schnorrq, crypto.K12);
        var destPublicKey = PublicKeyStringToBytes(destPublicId).AsSpan(0, PUBLIC_KEY_LENGTH).ToArray();

        var tx = new byte[TRANSACTION_SIZE];
        var txView = new DataView(tx);

        int offset = 0;
        Array.Copy(sourcePublicKey, 0, tx, offset, PUBLIC_KEY_LENGTH);
        offset += PUBLIC_KEY_LENGTH;

        Array.Copy(destPublicKey, 0, tx, offset, PUBLIC_KEY_LENGTH);
        offset += PUBLIC_KEY_LENGTH;

        txView.SetBigInt64(offset, amount, true);
        offset += 8;

        txView.SetInt32(offset, tick, true);
        offset += 4;

        txView.SetInt16(offset, 0, true);
        offset += 2;

        txView.SetInt16(offset, 0, true);
        offset += 2;

        var digest = new byte[DIGEST_LENGTH];
        var toSign = new byte[offset];
        Array.Copy(tx, toSign, offset);

        crypto.K12(toSign, digest, DIGEST_LENGTH);
        var signedTx = crypto.Schnorrq.Sign(sourcePrivateKey, sourcePublicKey, digest);

        Array.Copy(signedTx, 0, tx, offset, SIGNATURE_LENGTH);
        offset += SIGNATURE_LENGTH;

        return tx;
    }

    private byte[] GetIncreasingNonceAndCommandType(int type)
    {
        var timestamp = (ulong)(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        var commandByte = (ulong)type << 56;
        var result = commandByte | timestamp;

        var byteArray = new byte[8];
        var txView = new DataView(byteArray);
        txView.SetBigUint64(0, result, true);
        return byteArray;
    }

    public async Task<byte[]> CreateProposal(int protocol, int computorIndex, string operatorSeed, string url)
    {
        var crypto = await Crypto.GetInstance();
        var operatorPrivateKey = PrivateKey(operatorSeed, 0, crypto.K12);
        var operatorPublicKey = CreatePublicKey(operatorPrivateKey, crypto.Schnorrq, crypto.K12);

        var encoder = new UTF8Encoding();
        var urlBytes = encoder.GetBytes(url);
        var uri = new byte[255];
        Array.Copy(urlBytes, uri, urlBytes.Length);

        var proposal = new byte[SET_PROPOSAL_AND_BALLOT_REQUEST_SIZE + REQUEST_RESPONSE_HEADER_SIZE];
        var txView = new DataView(proposal);
        int offset = 0;

        int size = 600;
        proposal[0] = (byte)size;
        proposal[1] = (byte)(size >> 8);
        proposal[2] = (byte)(size >> 16);
        offset += 3;

        proposal[offset++] = PROCESS_SPECIAL_COMMAND;

        proposal[offset++] = (byte)new Random().Next(255);
        proposal[offset++] = (byte)new Random().Next(255);
        proposal[offset++] = (byte)new Random().Next(255);

        proposal[offset++] = PROCESS_SPECIAL_COMMAND;

        var timeStamp = GetIncreasingNonceAndCommandType(SPECIAL_COMMAND_SET_PROPOSAL_AND_BALLOT_REQUEST);
        Array.Copy(timeStamp, 0, proposal, offset, timeStamp.Length);
        offset += timeStamp.Length;

        txView.SetInt16(offset, (short)computorIndex, true);
        offset += 2;

        offset += 6;

        proposal[offset++] = (byte)urlBytes.Length;
        Array.Copy(uri, 0, proposal, offset, 255);
        offset += 255;

        offset += 256;

        var digest = new byte[DIGEST_LENGTH];
        var toSign = new byte[offset - REQUEST_RESPONSE_HEADER_SIZE];
        Array.Copy(proposal, REQUEST_RESPONSE_HEADER_SIZE, toSign, 0, toSign.Length);

        crypto.K12(toSign, digest, DIGEST_LENGTH);
        var signature = crypto.Schnorrq.Sign(operatorPrivateKey, operatorPublicKey, digest);

        Array.Copy(signature, 0, proposal, offset, SIGNATURE_LENGTH);
        offset += SIGNATURE_LENGTH;

        return proposal;
    }

    private byte[] VotesToByteArray(int[] votes)
    {
        var bitArray = new List<int>();

        for (int computorIndex = 0; computorIndex < votes.Length; computorIndex++)
        {
            var vote = votes[computorIndex];

            for (int i = 0; i < 3; i++)
            {
                var bit = (vote >> i) & 1;
                bitArray.Add(bit);
            }
        }

        var output = new byte[(bitArray.Count + 7) / 8];

        for (int k = 0; k < bitArray.Count; k += 8)
        {
            var byteIndex = k / 8;
            byte byteValue = 0;
            for (int j = 0; j < 8; j++)
            {
                var bit = k + j < bitArray.Count ? bitArray[k + j] : 0;
                byteValue |= (byte)(bit << j);
            }
            output[byteIndex] = byteValue;
        }
        return output;
    }

    public async Task<byte[][]> CreateBallotRequests(int protocol, string operatorSeed, int[] computorIndices, int[] votes)
    {
        var crypto = await Crypto.GetInstance();
        var output = new List<byte[]>();

        var operatorPrivateKey = PrivateKey(operatorSeed, 0, crypto.K12);
        var operatorPublicKey = CreatePublicKey(operatorPrivateKey, crypto.Schnorrq, crypto.K12);

        for (int index = 0; index < computorIndices.Length; index++)
        {
            var proposal = new byte[SET_PROPOSAL_AND_BALLOT_REQUEST_SIZE + REQUEST_RESPONSE_HEADER_SIZE];
            var txView = new DataView(proposal);
            int offset = 0;

            int size = 600;
            proposal[0] = (byte)size;
            proposal[1] = (byte)(size >> 8);
            proposal[2] = (byte)(size >> 16);
            offset += 3;

            proposal[offset++] = PROCESS_SPECIAL_COMMAND;

            proposal[offset++] = (byte)new Random().Next(255);
            proposal[offset++] = (byte)new Random().Next(255);
            proposal[offset++] = (byte)new Random().Next(255);

            proposal[offset++] = PROCESS_SPECIAL_COMMAND;

            var timeStamp = GetIncreasingNonceAndCommandType(SPECIAL_COMMAND_SET_PROPOSAL_AND_BALLOT_REQUEST);
            Array.Copy(timeStamp, 0, proposal, offset, timeStamp.Length);
            offset += timeStamp.Length;

            txView.SetInt16(offset, (short)computorIndices[index], true);
            offset += 2;

            offset += 6;

            proposal[offset++] = 0;
            offset += 255;

            offset++;

            var voteBytes = VotesToByteArray(votes);
            Array.Copy(voteBytes, 0, proposal, offset, voteBytes.Length);
            offset += voteBytes.Length;
            offset++;

            var digest = new byte[DIGEST_LENGTH];
            var toSign = new byte[offset - REQUEST_RESPONSE_HEADER_SIZE];
            Array.Copy(proposal, REQUEST_RESPONSE_HEADER_SIZE, toSign, 0, toSign.Length);

            crypto.K12(toSign, digest, DIGEST_LENGTH);
            var signature = crypto.Schnorrq.Sign(operatorPrivateKey, operatorPublicKey, digest);

            Array.Copy(signature, 0, proposal, offset, SIGNATURE_LENGTH);
            offset += SIGNATURE_LENGTH;

            output.Add(proposal);
        }
        return output.ToArray();
    }

    private void DownloadBlob(string fileName, byte[] blob)
    {
        var anchor = new System.Windows.Forms.SaveFileDialog
        {
            FileName = fileName,
            Filter = "All files (*.*)|*.*"
        };

        if (anchor.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            System.IO.File.WriteAllBytes(anchor.FileName, blob);
        }
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

    public void SetBigInt64(int byteOffset, long value, bool littleEndian)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian != littleEndian)
        {
            Array.Reverse(bytes);
        }
        Array.Copy(bytes, 0, buffer, byteOffset, 8);
    }

    public void SetInt32(int byteOffset, int value, bool littleEndian)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian != littleEndian)
        {
            Array.Reverse(bytes);
        }
        Array.Copy(bytes, 0, buffer, byteOffset, 4);
    }

    public void SetInt16(int byteOffset, short value, bool littleEndian)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian != littleEndian)
        {
            Array.Reverse(bytes);
        }
        Array.Copy(bytes, 0, buffer, byteOffset, 2);
    }
}


