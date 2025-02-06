using System;

public class Long : IQubicBuildPackage
{
    private long value = 0;

    public Long(long initialValue = 0)
    {
        SetNumber(initialValue);
    }

    public Long(byte[] initialValue)
    {
        if (initialValue.Length != 8)
        {
            throw new ArgumentException("Byte array must be exactly 8 bytes long");
        }
        value = BitConverter.ToInt64(initialValue, 0);
    }

    public void SetNumber(long n)
    {
        value = n;
    }

    public long GetNumber()
    {
        return value;
    }

    public int GetPackageSize()
    {
        return 8; // fixed size
    }

    public byte[] GetPackageData()
    {
        return BitConverter.GetBytes(value);
    }
}


