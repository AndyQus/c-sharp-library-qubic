public class DynamicPayload
{
    private byte[] bytes;
    private int filledSize = 0;
    private int maxSize = 0;

    /* Constructor */
    public DynamicPayload(int maxSize)
    {
        this.bytes = new byte[maxSize];
        this.maxSize = maxSize;
    }

    public DynamicPayload()
    {
    }

    /* Sets the payload */
    public void SetPayload(byte[] data)
    {
        if (data.Length > this.maxSize)
        {
            throw new ArgumentException($"data must be lower or equal {this.maxSize}");
        }

        this.bytes = data;
        this.filledSize = this.bytes.Length;
    }

    /* Gets the package data */
    public byte[] GetPackageData()
    {
        if (this.filledSize == 0)
        {
            return new byte[0];
        }
        return this.bytes;
    }

    /* Gets the package size */
    public int GetPackageSize()
    {
        return this.filledSize;
    }
}

