using System;

public class QubicEntity : IQubicBuildPackage
{
    private const int _internalPackageSize = 64;

    private PublicKey publicKey = new PublicKey();
    private Long incomingAmount = new Long();
    private Long outgoingAmount = new Long();
    private int numberOfIncomingTransfers = 0;
    private int numberOfOutgoingTransfers = 0;
    private int latestIncomingTransferTick = 0;
    private int latestOutgoingTransferTick = 0;

    public PublicKey GetPublicKey()
    {
        return publicKey;
    }

    public void SetPublicKey(PublicKey publicKey)
    {
        this.publicKey = publicKey;
    }

    public Long GetIncomingAmount()
    {
        return incomingAmount;
    }

    public void SetIncomingAmount(Long incomingAmount)
    {
        this.incomingAmount = incomingAmount;
    }

    public Long GetOutgoingAmount()
    {
        return outgoingAmount;
    }

    public void SetOutgoingAmount(Long outgoingAmount)
    {
        this.outgoingAmount = outgoingAmount;
    }

    public int GetNumberOfIncomingTransfers()
    {
        return numberOfIncomingTransfers;
    }

    public void SetNumberOfIncomingTransfers(int numberOfIncomingTransfers)
    {
        this.numberOfIncomingTransfers = numberOfIncomingTransfers;
    }

    public int GetNumberOfOutgoingTransfers()
    {
        return numberOfOutgoingTransfers;
    }

    public void SetNumberOfOutgoingTransfers(int numberOfOutgoingTransfers)
    {
        this.numberOfOutgoingTransfers = numberOfOutgoingTransfers;
    }

    public int GetLatestIncomingTransferTick()
    {
        return latestIncomingTransferTick;
    }

    public void SetLatestIncomingTransferTick(int latestIncomingTransferTick)
    {
        this.latestIncomingTransferTick = latestIncomingTransferTick;
    }

    public int GetLatestOutgoingTransferTick()
    {
        return latestOutgoingTransferTick;
    }

    public void SetLatestOutgoingTransferTick(int latestOutgoingTransferTick)
    {
        this.latestOutgoingTransferTick = latestOutgoingTransferTick;
    }

    public QubicEntity()
    {
    }

    public long GetBalance()
    {
        return incomingAmount.GetNumber() - outgoingAmount.GetNumber();
    }

    public int GetPackageSize()
    {
        return _internalPackageSize;
    }

    public QubicEntity Parse(byte[] data)
    {
        if (data.Length != _internalPackageSize)
        {
            Console.Error.WriteLine("INVALID PACKAGE SIZE");
            return null;
        }
        var dataView = new DataView(data);
        int offset = 0;
        SetPublicKey(new PublicKey(data.AsSpan(0, QubicDefinitions.PUBLIC_KEY_LENGTH).ToArray()));
        offset += QubicDefinitions.PUBLIC_KEY_LENGTH;
        SetIncomingAmount(new Long(dataView.GetInt64(offset, true)));
        offset += 8;
        SetOutgoingAmount(new Long(dataView.GetInt64(offset, true)));
        offset += 8;
        SetNumberOfIncomingTransfers(dataView.GetInt32(offset, true));
        offset += 4;
        SetNumberOfOutgoingTransfers(dataView.GetInt32(offset, true));
        offset += 4;
        SetLatestIncomingTransferTick(dataView.GetInt32(offset, true));
        offset += 4;
        SetLatestOutgoingTransferTick(dataView.GetInt32(offset, true));
        offset += 4;
        return this;
    }

    public byte[] GetPackageData()
    {
        var builder = new QubicPackageBuilder(_internalPackageSize);
        builder.Add(publicKey);
        builder.Add(incomingAmount);
        builder.Add(outgoingAmount);
        builder.AddInt(numberOfIncomingTransfers);
        builder.AddInt(numberOfOutgoingTransfers);
        builder.AddInt(latestIncomingTransferTick);
        builder.AddInt(latestOutgoingTransferTick);

        return builder.GetData();
    }
}


