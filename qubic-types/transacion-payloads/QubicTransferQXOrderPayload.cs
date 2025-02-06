using System;
using System.Text;
using System.Threading.Tasks;

public class QubicTransferQXOrderPayload : IQubicBuildPackage
{
    private const int _internalPackageSize = 56; // 32 + 8 + 8 + 8 -> 56

    private QXOrderActionInput qxOrderActionInput;

    public QubicTransferQXOrderPayload(QXOrderActionInput actionInput)
    {
        this.qxOrderActionInput = actionInput;
    }

    public int GetPackageSize()
    {
        return _internalPackageSize;
    }

    public byte[] GetPackageData()
    {
        var builder = new QubicPackageBuilder(GetPackageSize());

        builder.Add(qxOrderActionInput.Issuer);
        builder.Add(qxOrderActionInput.AssetName);
        builder.Add(qxOrderActionInput.Price);
        builder.Add(qxOrderActionInput.NumberOfShares);

        return builder.GetData();
    }

    public DynamicPayload GetTransactionPayload()
    {
        var payload = new DynamicPayload(GetPackageSize());
        payload.SetPayload(GetPackageData());
        return payload;
    }

    public long GetTotalAmount()
    {
        return qxOrderActionInput.Price.GetNumber() * qxOrderActionInput.NumberOfShares.GetNumber();
    }
}

public class QXOrderActionInput
{
    public PublicKey Issuer { get; set; }
    public Long AssetName { get; set; }
    public Long Price { get; set; }
    public Long NumberOfShares { get; set; }
}


