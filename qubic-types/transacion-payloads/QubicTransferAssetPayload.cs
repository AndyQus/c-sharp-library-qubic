using System;
using System.Text;
using System.Threading.Tasks;

public class QubicTransferAssetPayload : IQubicBuildPackage
{
    private const int _internalPackageSize = 32 + 32 + 8 + 8;

    private PublicKey issuer;
    private PublicKey newOwnerAndPossessor;
    private byte[] assetName;
    private Long numberOfUnits;

    public QubicTransferAssetPayload()
    {
    }

    public QubicTransferAssetPayload SetIssuer(PublicKey issuer)
    {
        this.issuer = issuer;
        return this;
    }

    public QubicTransferAssetPayload SetIssuer(string issuer)
    {
        this.issuer = new PublicKey(issuer);
        return this;
    }

    public QubicTransferAssetPayload SetNewOwnerAndPossessor(PublicKey newOwnerAndPossessor)
    {
        this.newOwnerAndPossessor = newOwnerAndPossessor;
        return this;
    }

    public QubicTransferAssetPayload SetNewOwnerAndPossessor(string newOwnerAndPossessor)
    {
        this.newOwnerAndPossessor = new PublicKey(newOwnerAndPossessor);
        return this;
    }

    public QubicTransferAssetPayload SetAssetName(byte[] assetName)
    {
        this.assetName = assetName;
        return this;
    }

    public QubicTransferAssetPayload SetAssetName(string assetName)
    {
        var utf8Encode = new UTF8Encoding();
        var nameBytes = utf8Encode.GetBytes(assetName);
        this.assetName = new byte[8];
        Array.Copy(nameBytes, this.assetName, Math.Min(nameBytes.Length, 8));
        return this;
    }

    public byte[] GetAssetName()
    {
        return assetName;
    }

    public PublicKey GetIssuer()
    {
        return issuer;
    }

    public PublicKey GetNewOwnerAndPossessor()
    {
        return newOwnerAndPossessor;
    }

    public Long GetNumberOfUnits()
    {
        return numberOfUnits;
    }

    public QubicTransferAssetPayload SetNumberOfUnits(Long numberOfUnits)
    {
        this.numberOfUnits = numberOfUnits;
        return this;
    }

    public QubicTransferAssetPayload SetNumberOfUnits(long numberOfUnits)
    {
        this.numberOfUnits = new Long(numberOfUnits);
        return this;
    }

    public int GetPackageSize()
    {
        return _internalPackageSize;
    }

    public byte[] GetPackageData()
    {
        var builder = new QubicPackageBuilder(GetPackageSize());
        builder.Add(issuer);
        builder.Add(newOwnerAndPossessor);
        builder.AddRaw(assetName);
        builder.Add(numberOfUnits);
        return builder.GetData();
    }

    public DynamicPayload GetTransactionPayload()
    {
        var payload = new DynamicPayload(GetPackageSize());
        payload.SetPayload(GetPackageData());
        return payload;
    }

    public async Task<QubicTransferAssetPayload> Parse(byte[] data)
    {
        if (data.Length != _internalPackageSize)
        {
            Console.Error.WriteLine("INVALID PACKAGE SIZE");
            return null;
        }

        var helper = new QubicHelper();

        int start = 0;
        int end = 32;

        issuer = new PublicKey(await helper.GetIdentity(data.AsSpan(start, end).ToArray()));

        start = end;
        end = start + 32;
        newOwnerAndPossessor = new PublicKey(await helper.GetIdentity(data.AsSpan(start, end).ToArray()));

        start = end;
        end = start + 8;
        assetName = data.AsSpan(start, end).ToArray();

        start = end;
        end = start + 8;
        numberOfUnits = new Long(data.AsSpan(start, end).ToArray());

        return this;
    }
}

