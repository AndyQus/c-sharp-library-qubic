using System;

public class QubicTickData : IQubicBuildPackage
{
    private const int _internalPackageSize = 41328;

    private DataView _unionDataView;
    public DataView UnionDataView
    {
        get
        {
            if (_unionDataView == null)
                _unionDataView = new DataView(UnionData);
            return _unionDataView;
        }
        set
        {
            _unionDataView = value;
        }
    }

    private ushort computorIndex;
    private ushort epoch;
    private uint tick;

    private ushort millisecond;
    private byte second;
    private byte minute;

    private byte hour;
    private byte day;
    private byte month;
    private byte year;

    private byte[] unionData = new byte[256];

    private byte[] timeLock = new byte[32];
    private byte[] transactionDigests = new byte[QubicDefinitions.NUMBER_OF_TRANSACTIONS_PER_TICK * QubicDefinitions.DIGEST_LENGTH];

    private long[] contractFees = new long[QubicDefinitions.MAX_NUMBER_OF_CONTRACTS];

    private Signature signature;

    public Signature GetSignature()
    {
        return signature;
    }

    public void SetSignature(Signature signature)
    {
        this.signature = signature;
    }

    public ushort GetComputorIndex()
    {
        return computorIndex;
    }

    public void SetComputorIndex(ushort computorIndex)
    {
        this.computorIndex = computorIndex;
    }

    public ushort GetEpoch()
    {
        return epoch;
    }

    public void SetEpoch(ushort epoch)
    {
        this.epoch = epoch;
    }

    public uint GetTick()
    {
        return tick;
    }

    public void SetTick(uint tick)
    {
        this.tick = tick;
    }

    public ushort GetMillisecond()
    {
        return millisecond;
    }

    public void SetMillisecond(ushort millisecond)
    {
        this.millisecond = millisecond;
    }

    public byte GetSecond()
    {
        return second;
    }

    public void SetSecond(byte second)
    {
        this.second = second;
    }

    public byte GetMinute()
    {
        return minute;
    }

    public void SetMinute(byte minute)
    {
        this.minute = minute;
    }

    public byte GetHour()
    {
        return hour;
    }

    public void SetHour(byte hour)
    {
        this.hour = hour;
    }

    public byte GetDay()
    {
        return day;
    }

    public void SetDay(byte day)
    {
        this.day = day;
    }

    public byte GetMonth()
    {
        return month;
    }

    public void SetMonth(byte month)
    {
        this.month = month;
    }

    public byte GetYear()
    {
        return year;
    }

    public void SetYear(byte year)
    {
        this.year = year;
    }

    public byte[] GetUnionData()
    {
        return unionData;
    }

    public void SetUnionData(byte[] unionData)
    {
        this.unionData = unionData;
    }

    public byte[] GetTimeLock()
    {
        return timeLock;
    }

    public void SetTimeLock(byte[] timeLock)
    {
        this.timeLock = timeLock;
    }

    public void SetTransactionDigests(byte[] transactionDigests)
    {
        this.transactionDigests = transactionDigests;
    }

    public long[] GetContractFees()
    {
        return contractFees;
    }

    public void SetContractFees(long[] contractFees)
    {
        this.contractFees = contractFees;
    }

    public ushort GetProposalUriSize()
    {
        return unionData[0];
    }

    public void SetProposalUriSize(ushort size)
    {
        unionData[0] = (byte)size;
    }

    public string GetProposalUri()
    {
        return Encoding.UTF8.GetString(unionData, 1, GetProposalUriSize());
    }

    public void SetProposalUri(string uri)
    {
        if (uri.Length > 255)
        {
            Console.Error.WriteLine("URI SIZE MUST BE MAX 255");
            throw new ArgumentException("URI SIZE MUST BE MAX 255");
        }

        var bytes = Encoding.UTF8.GetBytes(uri);
        Array.Copy(bytes, 0, unionData, 1, bytes.Length);
        SetProposalUriSize((ushort)uri.Length);
    }

    public QubicTickData()
    {
    }

    public int GetPackageSize()
    {
        return _internalPackageSize;
    }

    public QubicTickData Parse(byte[] data)
    {
        if (data.Length != _internalPackageSize)
        {
            Console.Error.WriteLine("INVALID PACKAGE SIZE");
            return null;
        }
        var dataView = new DataView(data);
        int offset = 0;

        SetComputorIndex(dataView.GetUInt16(offset, true));
        offset += 2;

        SetEpoch(dataView.GetUInt16(offset, true));
        offset += 2;

        SetTick(dataView.GetUInt32(offset, true));
        offset += 4;

        SetMillisecond(dataView.GetUInt16(offset, true));
        offset += 2;

        SetSecond(data[offset++]);
        SetMinute(data[offset++]);
        SetHour(data[offset++]);
        SetDay(data[offset++]);
        SetMonth(data[offset++]);
        SetYear(data[offset++]);

        SetUnionData(data.AsSpan(offset, 256).ToArray());
        offset += 256;

        SetTimeLock(data.AsSpan(offset, 32).ToArray());
        offset += 32;

        SetTransactionDigests(data.AsSpan(offset, QubicDefinitions.NUMBER_OF_TRANSACTIONS_PER_TICK * QubicDefinitions.DIGEST_LENGTH).ToArray());
        offset += QubicDefinitions.NUMBER_OF_TRANSACTIONS_PER_TICK * QubicDefinitions.DIGEST_LENGTH;

        var contractFees = new long[QubicDefinitions.MAX_NUMBER_OF_CONTRACTS];
        for (int i = 0; i < QubicDefinitions.MAX_NUMBER_OF_CONTRACTS; i++)
        {
            contractFees[i] = dataView.GetInt64(offset, true);
            offset += 8;
        }
        SetContractFees(contractFees);

        SetSignature(new Signature(data.AsSpan(offset, QubicDefinitions.SIGNATURE_LENGTH).ToArray()));
        offset += QubicDefinitions.SIGNATURE_LENGTH;

        return this;
    }

    public byte[] GetPackageData()
    {
        // todo: implement

        return new byte[0];
    }
}



