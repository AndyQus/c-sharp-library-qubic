using System;
using System.Text;
using System.Threading.Tasks;

public class QubicTransaction : IQubicBuildPackage
{
    private byte[] builtData;
    public byte[] Digest { get; private set; }
    public string Id { get; private set; }

    public PublicKey SourcePublicKey { get; private set; } = new PublicKey();
    public PublicKey DestinationPublicKey { get; private set; } = new PublicKey();
    public Long Amount { get; private set; } = new Long();
    public int Tick { get; private set; } = 0;
    public int InputType { get; private set; } = 0;
    public int InputSize { get; private set; } = 0;

    public IQubicBuildPackage Payload { get; private set; } = new DynamicPayload(QubicDefinitions.MAX_TRANSACTION_SIZE);
    public Signature Signature { get; private set; } = new Signature();

    public QubicTransaction SetSourcePublicKey(PublicKey p)
    {
        SourcePublicKey = p;
        return this;
    }

    public QubicTransaction SetSourcePublicKey(string p)
    {
        SourcePublicKey = new PublicKey(p);
        return this;
    }

    public QubicTransaction SetDestinationPublicKey(PublicKey p)
    {
        DestinationPublicKey = p;
        return this;
    }

    public QubicTransaction SetDestinationPublicKey(string p)
    {
        DestinationPublicKey = new PublicKey(p);
        return this;
    }

    public QubicTransaction SetAmount(Long p)
    {
        Amount = p;
        return this;
    }

    public QubicTransaction SetAmount(long p)
    {
        Amount = new Long(p);
        return this;
    }

    public QubicTransaction SetTick(int p)
    {
        Tick = p;
        return this;
    }

    public QubicTransaction SetInputType(int p)
    {
        InputType = p;
        return this;
    }

    public QubicTransaction SetInputSize(int p)
    {
        InputSize = p;
        return this;
    }

    public QubicTransaction SetPayload(IQubicBuildPackage payload)
    {
        Payload = payload;
        InputSize = Payload.GetPackageSize();
        return this;
    }

    public IQubicBuildPackage GetPayload()
    {
        return Payload;
    }

    private int InternalSize()
    {
        return SourcePublicKey.GetPackageSize()
            + DestinationPublicKey.GetPackageSize()
            + Amount.GetPackageSize()
            + 4 // tick
            + 2 // inputType
            + 2 // inputSize
            + InputSize
            + Signature.GetPackageSize();
    }

    public int GetPackageSize()
    {
        return InternalSize();
    }

    public string GetId()
    {
        if (Id == null)
        {
            Console.Error.WriteLine("CALL build() BEFORE USING getId() METHOD");
            return "";
        }
        return Id;
    }

    public async Task<byte[]> Build(string seed)
    {
        builtData = null;
        var builder = new QubicPackageBuilder(InternalSize());
        builder.Add(SourcePublicKey);
        builder.Add(DestinationPublicKey);
        builder.Add(Amount);
        builder.AddInt(Tick);
        builder.AddShort(InputType);
        builder.AddShort(InputSize);
        builder.Add(Payload);
        var (signedData, digest, signature) = await builder.SignAndDigest(seed);
        builtData = signedData;
        Digest = digest;
        Signature = new Signature(signature);
        Id = await new QubicHelper().GetHumanReadableBytes(digest);
        return signedData;
    }

    public string EncodeTransactionToBase64(byte[] transaction)
    {
        return Convert.ToBase64String(transaction);
    }

    public byte[] GetPackageData()
    {
        if (builtData == null)
        {
            Console.Error.WriteLine("CALL build() BEFORE USING getPackageData() METHOD");
        }
        return builtData ?? new byte[0];
    }
}


