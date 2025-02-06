using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class QubicTransferSendManyPayload : IQubicBuildPackage
{
    private const int _internalPackageSize = 1000; /* 25 * 32  + 25 * 8 */

    // max 25 transfers allowed
    private List<SendManyTransfer> sendManyTransfers = new List<SendManyTransfer>();

    public QubicTransferSendManyPayload() { }

    public QubicTransferSendManyPayload AddTransfer(SendManyTransfer transfer)
    {
        if (sendManyTransfers.Count < 25)
        {
            sendManyTransfers.Add(transfer);
        }
        else
        {
            throw new ArgumentException("max 25 send many transfers allowed");
        }
        return this;
    }

    public QubicTransferSendManyPayload AddTransfers(IEnumerable<SendManyTransfer> transfers)
    {
        if (sendManyTransfers.Count + transfers.Count() > 25)
        {
            throw new ArgumentException("max 25 send many transfers allowed");
        }
        foreach (var transfer in transfers)
        {
            AddTransfer(transfer);
        }
        return this;
    }

    public List<SendManyTransfer> GetTransfers()
    {
        return sendManyTransfers;
    }

    public long GetTotalAmount()
    {
        long totalAmount = 0;
        foreach (var transfer in sendManyTransfers)
        {
            totalAmount += transfer.Amount.GetNumber();
        }
        return totalAmount;
    }

    public int GetPackageSize()
    {
        return _internalPackageSize;
    }

    public byte[] GetPackageData()
    {
        var builder = new QubicPackageBuilder(GetPackageSize());
        for (int i = 0; i < 25; i++)
        {
            if (sendManyTransfers.Count > i && sendManyTransfers[i].Amount.GetNumber() > 0)
            {
                builder.Add(sendManyTransfers[i].DestId);
            }
            else
            {
                builder.Add(new PublicKey(QubicDefinitions.EMPTY_ADDRESS)); // add empty address to have 0 in byte
            }
        }
        for (int i = 0; i < 25; i++)
        {
            if (sendManyTransfers.Count > i && sendManyTransfers[i].Amount.GetNumber() > 0)
            {
                builder.Add(sendManyTransfers[i].Amount);
            }
            else
            {
                builder.Add(new Long(0));
            }
        }
        return builder.GetData();
    }

    public DynamicPayload GetTransactionPayload()
    {
        var payload = new DynamicPayload(GetPackageSize());
        payload.SetPayload(GetPackageData());
        return payload;
    }

    public async Task<QubicTransferSendManyPayload> Parse(byte[] data)
    {
        if (data.Length != _internalPackageSize)
        {
            Console.Error.WriteLine("INVALID PACKAGE SIZE");
            return null;
        }

        var helper = new QubicHelper();

        var sendManyTransfers = new List<SendManyTransfer>();

        for (int i = 0; i < 25; i++)
        {
            var amount = new Long(data.AsSpan(800 + i * 8, 8).ToArray());
            if (amount.GetNumber() > 0)
            {
                var dest = data.AsSpan(32 * i, 32).ToArray();
                sendManyTransfers.Add(new SendManyTransfer
                {
                    Amount = amount,
                    DestId = new PublicKey(await helper.GetIdentity(dest))
                });
            }
        }

        AddTransfers(sendManyTransfers);

        return this;
    }
}

public class SendManyTransfer
{
    public PublicKey DestId { get; set; }
    public Long Amount { get; set; }
}


