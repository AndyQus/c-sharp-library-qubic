public class QubicTickInfo : IQubicBuildPackage
{
    private const int _internalPackageSize = 16;

    private ushort tickDuration = 0;
    private ushort epoch = 0;
    private uint tick = 0;
    private ushort numberOfAlignedVotes = 0;
    private ushort numberOfMisalignedVotes = 0;
    private uint initialTick = 0;

    public ushort GetTickDuration()
    {
        return tickDuration;
    }

    public void SetTickDuration(ushort tickDuration)
    {
        this.tickDuration = tickDuration;
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

    public ushort GetNumberOfAlignedVotes()
    {
        return numberOfAlignedVotes;
    }

    public void SetNumberOfAlignedVotes(ushort numberOfAlignedVotes)
    {
        this.numberOfAlignedVotes = numberOfAlignedVotes;
    }

    public ushort GetNumberOfMisalignedVotes()
    {
        return numberOfMisalignedVotes;
    }

    public void SetNumberOfMisalignedVotes(ushort numberOfMisalignedVotes)
    {
        this.numberOfMisalignedVotes = numberOfMisalignedVotes;
    }

    public uint GetInitialTick()
    {
        return initialTick;
    }

    public void SetInitialTick(uint initialTick)
    {
        this.initialTick = initialTick;
    }

    public QubicTickInfo()
    {
    }

    public int GetPackageSize()
    {
        return _internalPackageSize;
    }

    public QubicTickInfo Parse(byte[] data)
    {
        if (data.Length != _internalPackageSize)
        {
            Console.Error.WriteLine("INVALID PACKAGE SIZE");
            return null;
        }
        var dataView = new DataView(data);
        int offset = 0;
        SetTickDuration(dataView.GetUInt16(offset, true));
        offset += 2;
        SetEpoch(dataView.GetUInt16(offset, true));
        offset += 2;
        SetTick(dataView.GetUInt32(offset, true));
        offset += 4;
        SetNumberOfAlignedVotes(dataView.GetUInt16(offset, true));
        offset += 2;
        SetNumberOfMisalignedVotes(dataView.GetUInt16(offset, true));
        offset += 2;
        SetInitialTick(dataView.GetUInt32(offset, true));
        return this;
    }

    public byte[] GetPackageData()
    {
        var builder = new QubicPackageBuilder(_internalPackageSize);
        builder.AddShort(tickDuration);
        builder.AddShort(epoch);
        builder.AddInt(tick);
        builder.AddShort(numberOfAlignedVotes);
        builder.AddShort(numberOfMisalignedVotes);
        builder.AddInt(initialTick);
        return builder.GetData();
    }
}



